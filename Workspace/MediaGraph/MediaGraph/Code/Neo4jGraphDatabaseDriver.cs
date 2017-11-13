using MediaGraph.Models;
using MediaGraph.Models.Component;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace MediaGraph.Code
{
    public class Neo4jGraphDatabaseDriver : IGraphDatabaseDriver
    {
        private const string kNodeCreationStatement = "CREATE (n:{0} $props) ";
        private const string kMatchNodeQuery = "MATCH (n {id: $id}) RETURN n";
        private const string kDeleteNodeQuery = "MATCH (n {id: $id}) DETACH DELETE n";
        private const string kMatchNodeAndRelationshipsQuery = "MATCH (n {id: $id})-[r]-(e) RETURN DISTINCT n, r, e";
        private const string kMatchPathQuery = "MATCH p = ({id: $id})--() RETURN DISTINCT p";

        private const string kCreateRelationshipStatement = "CREATE ({0})-[:Related{1} {2}]->({3}) ";

        private const string kMatchNodeByNameQuery = "MATCH (n {commonName: $name}) RETURN n";
        private const string kMatchPathsByNameQuery = "MATCH p = ({commonName: $name})--() RETURN p";

        //private const string kMatchNodeByAnyName = "MATCH (n) WHERE n.commonName = $name OR $name IN n.otherNames RETURN n";

        private readonly IDriver driver;
        private const bool kUseInternal = true;

        public Neo4jGraphDatabaseDriver()
        {
            // Create the database driver
            driver = GraphDatabase.Driver(kUseInternal ? ConfigurationManager.AppSettings["neoInternal"] : ConfigurationManager.AppSettings["neoExternal"],
                AuthTokens.Basic(ConfigurationManager.AppSettings["neoLogin"], ConfigurationManager.AppSettings["neoPass"]));
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
            builder.AppendLine();
            // Append the relationship statements
            BuildRelationshipMergeStatement(builder, model.Relationships, model.ContentType);
            // Append the return statement
            builder.Append("RETURN n");

            return builder.ToString();
        }

        private void BuildRelationshipMergeStatement(StringBuilder builder, IEnumerable<RelationshipModel> relationships, NodeContentType sourceType)
        {
            int targetIndex = 0;
            foreach(RelationshipModel relModel in relationships)
            {
                string identifier = $"targetRel{targetIndex}";
                builder.AppendFormat("MERGE ({0}:{1} ", identifier, relModel.GetNodeLabel());
                // Start the properties section
                builder.Append('{');
                // Append the properties
                if (relModel.TargetId != null && relModel.TargetId != Guid.Empty)
                    // We have the id
                    builder.AppendFormat("id:'{0}'", relModel.TargetId.ToString());
                else
                    // We do not have the id - create a new node
                    builder.AppendFormat("commonName:'{0}'", relModel.TargetName);
                // End the properties section
                builder.Append("}) ");
                builder.AppendLine();

                // If we merged based on the name,
                if(relModel.TargetId == null || relModel.TargetId == Guid.Empty)
                {
                    // Append an ON CREATE statement to get the created node an id
                    builder.AppendFormat("ON CREATE SET {0}.id = '{1}' ", identifier, Guid.NewGuid().ToString());
                }

                // Append the creation statement for the relationship
                string relProps = "{roles: " + JsonConvert.SerializeObject(relModel.Roles) + "}";
                // If this is a media node,
                if(sourceType == NodeContentType.Media)
                {
                    // This relationship goes Target -> Source
                    builder.AppendFormat(kCreateRelationshipStatement, identifier, relModel.GetNodeLabel(), relProps, "n");
                }
                else
                {
                    // This relationship goes Source -> Target
                    builder.AppendFormat(kCreateRelationshipStatement, "n", Enum.GetName(typeof(NodeContentType), sourceType), relProps, identifier);
                }
                builder.AppendLine();
                // Incremenet targetIndex
                targetIndex++;
            }
        }

        #region Update Methods
        public bool UpdateNode(BasicNodeModel model)
        {
            bool success = false;
            using (ISession session = driver.Session())
            {
                success = session.ReadTransaction(action =>
                {
                    IStatementResult result = action.Run(kMatchNodeQuery, new Dictionary<string, object> { { "id", model.Id.ToString() } });
                    // We found a node to delete
                    if(result.FirstOrDefault() != null)
                    {
                        // Delete the node
                        Delete(action, model.Id);
                    }
                    // Update the node
                    return Update(action, model);
                });
            }

            return success;
        }

        private bool Delete(ITransaction transaction, Guid id)
        {
            return transaction.Run(kDeleteNodeQuery, new Dictionary<string, object> { { "id", id.ToString() } }).Summary.Counters.NodesDeleted > 0;
        }

        private bool Update(ITransaction transaction, BasicNodeModel node)
        {
            return transaction.Run(BuildNodeCreationQuery(node), new { props = node.GetPropertyMap() }).Summary.Counters.NodesCreated > 0;
        }
        #endregion


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
                    IStatementResult result = action.Run(kDeleteNodeQuery, new Dictionary<string, object> { { "id", id.ToString() } });
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
                    return statementResult.FirstOrDefault()?[0].As<INode>();
                });
            }

            return result != null ? BasicNodeModel.FromINode(result) : null;
        }

        public BasicNodeModel GetNode(string commonName)
        {
            if (commonName == null)
                return null;

            INode result = null;

            using (ISession session = driver.Session())
            {
                result = session.ReadTransaction(action =>
                {
                    IStatementResult statementResult = action.Run(kMatchNodeByNameQuery, new Dictionary<string, object> { { "name", commonName } });
                    return statementResult.FirstOrDefault()?[0].As<INode>();
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

        public List<IPath> GetPaths(string commonName)
        {
            List<IPath> paths = new List<IPath>();

            using (ISession session = driver.Session())
            {
                // Create a transaction
                session.ReadTransaction(action =>
                {
                    // Run the query
                    IStatementResult result = action.Run(kMatchPathsByNameQuery, new { name = commonName });
                    // Add each of the paths to the result
                    foreach(IRecord record in result)
                    {
                        paths.Add(record[0].As<IPath>());
                    }
                });
            }

            return paths;
        }

        /// <summary>
        /// Returns a collection of nodes whose release date falls before
        /// </summary>
        /// <param name="start">The DateTime to use as the lower bound of the search</param>
        /// <param name="end">The DateTime to use as the upper bound of the search</param>
        /// <returns>A collection of nodes whose release date is in the specified range</returns>
        public List<BasicNodeModel> GetNodesBetweenDates(DateTime start, DateTime end)
        {
            List<BasicNodeModel> nodeModels = new List<BasicNodeModel>();
            using (ISession session = driver.Session())
            {
                // Create a transaction
                session.ReadTransaction(action =>
                {
                    IStatementResult result = action.Run("MATCH (n) WHERE (n.releaseDate >= $lowerBound AND n.releaseDate <= $upperBound) OR (n.deathDate >= $lowerBound AND n.deathDate <= $upperBound) RETURN n",
                        new Dictionary<string, object>
                        {
                            { "lowerBound", DateValueConverter.ToLongValue(start) },
                            { "upperBound", DateValueConverter.ToLongValue(end) }
                        });
                    // Add the nodes 
                    foreach(IRecord record in result)
                    {
                        nodeModels.Add(BasicNodeModel.FromINode(record[0].As<INode>()));
                    }
                });
            }

            return nodeModels;
        }

        public IRecord GetRelationship(Guid source, Guid target)
        {
            IRecord relationship = null;

            using (ISession session = driver.Session())
            {
                // Create a transaction
                relationship = session.ReadTransaction(action =>
                {
                    IStatementResult result = action.Run("MATCH (s {id: $sourceId})-[r]-(e {id: $targetId}) RETURN s.commonName, r.roles, e.commonName",
                        new Dictionary<string, object>
                        {
                            { "sourceId", source.ToString() },
                            { "targetId", target.ToString() }
                        });
                    return result.FirstOrDefault();
                });
            }

            return relationship;
        }

        #region Simple Autocomplete Functionality
        /// <summary>
        /// A simple implementation of autocomplete functionality using Neo4j itself.
        /// This simple implementation will not check for matches in the 'otherNames' property
        /// and might be very inefficient with large data sets.
        /// </summary>
        /// <param name="name">The name of the node for which to search</param>
        /// <returns>A collection of key value pairs that represent the nodes that match the given string</returns>
        public Dictionary<string, string> SearchForNodes(string name)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            using (ISession session = driver.Session())
            {
                session.ReadTransaction(action =>
                {
                    // Run the query
                    IStatementResult statementResult = action.Run("MATCH (n) WHERE LOWER(n.commonName) STARTS WITH $text RETURN n.commonName, n.id", new { text = name });
                    // Populate the dictionary
                    foreach(IRecord record in statementResult)
                    {
                        results.Add(record[0].As<string>(), record[1].As<string>());
                    }
                });
            }

            return results;
        }

        /// <summary>
        /// Searches for nodes of a specific type whose name starts with the given string
        /// </summary>
        /// <param name="contentType">The content type of the nodes to search</param>
        /// <param name="name">The name being searched</param>
        /// <returns>A collection of key-value pairs representing the matching nodes</returns>
        public Dictionary<string, string> SearchForNodes(NodeContentType contentType, string name)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            string searchType = Enum.GetName(typeof(NodeContentType), contentType);
            string queryBase = $"MATCH (n:{searchType}) WHERE n.commonName STARTS WITH $text RETURN n.commonName, n.id";
            using (ISession session = driver.Session())
            {
                session.ReadTransaction(action =>
                {
                    // Run the query
                    IStatementResult statementResult = action.Run(queryBase, new { text = name });
                    // Populate the result
                    foreach(IRecord record in statementResult)
                    {
                        results.Add(record[0].As<string>(), record[1].As<string>());
                    }
                });
            }

            return results;
        }
        #endregion
    }
}