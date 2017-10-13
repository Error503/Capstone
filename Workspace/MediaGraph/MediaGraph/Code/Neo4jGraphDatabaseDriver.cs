using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.Models.Util;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MediaGraph.Code
{
    public class Neo4jGraphDatabaseDriver : IGraphDatabaseDriver
    {
        private const string kNodeCreationStatement = "CREATE (n:{0} $props) ";
        private const string kMatchNodeQuery = "MATCH (n {id: $id}) RETURN n";
        private const string kDeleteNodeQuery = "MATCH (n {id: $id}) DETACH DELETE n";
        private const string kMatchNodeAndRelationshipsQuery = "MATCH (n {id: $id})-[r]-(e) RETURN DISTINCT n, r, e";
        private const string kMatchPathQuery = "MATCH p = ({id: $id})--() RETURN DISTINCT p";
        private const string kUpdateNodeQuery = "MATCH (update:{0} {1}) SET update = $props ";
        private const string kUpdateRelationshipQuery = "MATCH ({id: $id})-[r]-({id: $id2}) SET r.roles = $roles RETURN r";
        private const string kCreateOrUpdateRelationshipQuery = "MATCH (n1 {id: $id1) OPTIONAL MATCH (n2 {id: $id2}) MERGE (n1)<-[r:Related]-(n2) ON MATCH SET r = $props ON CREATE SET r = $prop RETURN n1, r, n2";

        private const string kCreateRelationshipStatement = "CREATE ({0})-[:Related{1} {2}]->({3}) ";

        private readonly IDriver driver;

        public Neo4jGraphDatabaseDriver()
        {
            // Create the database driver
            driver = GraphDatabase.Driver(Properties.Settings.Default.neo4jUrl, 
                AuthTokens.Basic(Properties.Settings.Default.neo4jLogin, Properties.Settings.Default.neo4jPass));
        }

        /// <summary>
        /// Implemenation of the Dispose method from IDisposeable
        /// </summary>
        public void Dispose()
        {
            driver?.Dispose();
        }

        /// <summary>
        /// Adds the given node to the graph. 
        /// PRECONDITION: The node must be valid
        /// </summary>
        /// <param name="node">The BasicNodeModel of the node to add to the graph</param>
        /// <returns>Returns true if the node was added to the graph successfully</returns>
        public bool AddNode(BasicNodeModel node)
        {
            INode sessionResult;

            // Create the session
            using (ISession session = driver.Session())
            {
                // Run the statement in a transaction
                sessionResult = session.WriteTransaction(action =>
                {
                    // Create the node
                    IStatementResult result = action.Run(BuildNodeCreationQuery(node), new { props = node.GetPropertyMap() });

                    return result.Single()[0].As<INode>();
                });
            }

            return sessionResult != null;
        }
        
        /// <summary>
        /// Creates a single large query for the creation of a node and it's direct relationships.
        /// </summary>
        /// <param name="model">The for which to create the creation query</param>
        /// <returns>The query string for the creation of the node and its direct relationships</returns>
        private string BuildNodeCreationQuery(BasicNodeModel model)
        {
            StringBuilder builder = new StringBuilder();

            // Append the node creation statement
            builder.AppendFormat(kNodeCreationStatement, model.GetNodeLabels());
            // Append the relationship statements
            BuildRelationshipMergeStatement(builder, model.RelatedCompanies, NodeContentType.Company, model.ContentType);
            BuildRelationshipMergeStatement(builder, model.RelatedMedia, NodeContentType.Media, model.ContentType);
            BuildRelationshipMergeStatement(builder, model.RelatedPeople, NodeContentType.Person, model.ContentType);
            // Append the return statement
            builder.Append("RETURN n");

            return builder.ToString();
        }

        private void BuildRelationshipMergeStatement(StringBuilder builder, IEnumerable<RelationshipModel> relationships, 
            NodeContentType targetType, NodeContentType sourceType)
        {
            int targetIndex = 0;
            string label = Enum.GetName(typeof(NodeContentType), targetType);
            foreach(RelationshipModel relModel in relationships)
            {
                string identifier = $"target{label}{targetIndex}";
                builder.AppendFormat("MERGE ({0}:{1} ", identifier, label);
                // Start the properties section
                builder.Append('{');
                // Append the properties
                if (relModel.TargetId != Guid.Empty)
                    // We have the id
                    builder.AppendFormat("id:'{0}'", relModel.TargetId.ToString());
                else
                    // We do not have the id - create a new node
                    builder.AppendFormat("id:'{0}',commonName:'{1}'", Guid.NewGuid().ToString(), relModel.TargetName);
                // End the properties section
                builder.Append("}) ");  

                // Append the creation statement for the relationship
                string relProps = "{roles: " + JsonConvert.SerializeObject(relModel.Roles) + "}";
                if (sourceType != targetType)
                    builder.AppendFormat(kCreateRelationshipStatement, identifier, label, relProps, "n");
                else
                    builder.AppendFormat(kCreateRelationshipStatement, "n", label, relProps, identifier);

                // Incremenet targetIndex
                targetIndex++;
            }
        }

        public bool UpdateNode(BasicNodeModel node, IEnumerable<RelationshipModel> updatedRelationships, IEnumerable<RelationshipModel> deletedRelationships)
        {
            int nodesUpdated = 0;
            int relsUpdated = 0;
            int relsDeleted = 0;
            using (ISession session = driver.Session())
            {
                // Start a transaction
                session.WriteTransaction(action =>
                {
                    // Update the node
                    IStatementResult nodeResult = action.Run(string.Format(kUpdateNodeQuery, node.GetNodeLabels(), "{id:'" + node.Id.ToString() + "'}"),
                        new Dictionary<string, object>
                        {
                            { "props", node.GetPropertyMap() }
                        });
                    nodesUpdated += nodeResult.Summary.Counters.PropertiesSet > 0 ? 1 : 0;
                    // Update the relationships
                    relsUpdated = UpdateRelationships(action, updatedRelationships);
                    // Delete the removed relationships
                    relsDeleted = DeleteRelationships(action, deletedRelationships);
                });
            }

            return nodesUpdated > 0;
        }

        private int UpdateRelationships(ITransaction transaction, IEnumerable<RelationshipModel> updated)
        {
            int relsUpdated = 0;
            foreach (RelationshipModel updateRel in updated)
            {
                IStatementResult result = transaction.Run("MATCH (update {id: $id1})-[r]-({id: $id2}) SET r.roles = $roles",
                    new Dictionary<string, object>
                    {
                                { "id1", updateRel.SourceId.ToString() },
                                { "id2", updateRel.TargetId.ToString() },
                                { "roles", updateRel.Roles }
                    });
                relsUpdated += result.Summary.Counters.RelationshipsCreated > 0 || result.Summary.Counters.PropertiesSet > 0 ? 1 : 0;
            }

            return relsUpdated;
        }

        private int DeleteRelationships(ITransaction transaction, IEnumerable<RelationshipModel> deleted)
        {
            int relsDeleted = 0;
            foreach (RelationshipModel deleteRel in deleted)
            {
                IStatementResult result = transaction.Run("MATCH ({id: $id1})-[r]-({id: $id2}) DELETE r",
                        new Dictionary<string, object>
                        {
                                    { "id1", deleteRel.SourceId.ToString() },
                                    { "id2", deleteRel.TargetId.ToString() }
                        });
                relsDeleted += result.Summary.Counters.RelationshipsDeleted;
            }

            return relsDeleted;
        }


        /// <summary>
        /// Deletes a node and the relationships associated with that node.
        /// </summary>
        /// <param name="id">The GUID of the node to remove</param>
        /// <returns>Returns true if the node was deleted successfully</returns>
        public bool DeleteNode(Guid id)
        {
            bool success = false;
            // Create a session
            using (ISession session = driver.Session())
            {
                // Create a transaction
                success = session.WriteTransaction(action =>
                {
                    IStatementResult result = action.Run(kDeleteNodeQuery, new { id = id.ToString() });
                    return result.Summary.Counters.NodesDeleted > 0;
                });
            }

            return success;
        }

        public BasicNodeModel GetNode(Guid id)
        {
            INode result;

            using (ISession session = driver.Session())
            {
                result = session.ReadTransaction(action =>
                {
                    IStatementResult statementResult = action.Run(kMatchNodeQuery, new Dictionary<string, object> { { "id", id.ToString() } });
                    return statementResult.Count() > 0 ? statementResult.Single()[0].As<INode>() : null;
                });
            }

            return result != null ? BasicNodeModel.FromINode(result) : null;
        }

        public BasicNodeModel GetNodeAndRelationships(Guid id)
        {
            BasicNodeModel node = null;

            using (ISession session = driver.Session())
            {
                session.ReadTransaction(action =>
                {
                    IStatementResult statementResult = action.Run(kMatchNodeAndRelationshipsQuery, new Dictionary<string, object> { { "id", id.ToString() } });
                    // Get the relationships
                    foreach(IRecord record in statementResult)
                    {
                        if(node == null)
                        {
                            node = BasicNodeModel.FromINode(record[0].As<INode>());
                        }
                        node.AddRelationship(record["r"].As<IRelationship>(), record["e"].As<INode>());
                    }
                });
            }

            return node;
        }

        public List<IPath> GetPaths(Guid id)
        {
            List<IPath> paths = new List<IPath>();

            using (ISession session = driver.Session())
            {
                // Create a transaction
                session.ReadTransaction(action =>
                {
                    // Run the query
                    IStatementResult result = action.Run(kMatchPathQuery, new { id = id.ToString() });
                    // Add each of the paths to return object
                    foreach(IRecord record in result)
                    {
                        paths.Add(record[0].As<IPath>());
                    }
                });
            }

            return paths;
        }
    }
}