using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models;
using Neo4j.Driver.V1;
using MediaGraph.Models.Util;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;

namespace MediaGraph.Code
{
    public class Neo4jGraphDatabaseDriver : IGraphDatabaseDriver
    {
        private const string kNodeCreationStatement = "CREATE (n{0} {1}) RETURN n";
        private const string kNodeParameters = "{id: $id, primaryName: $primaryName, date: $date, otherNames: $otherNames, franchise: $franchise, genres: $genres}";
        private const string kNodeDeleteStatement = "MATCH (n {id = $id}) DETATCH DELETE n";
        private const string kSingleNodeMatchStatement = "MATCH (n {id = $id}) RETURN n";
        private const string kRelationshipsMatchStatement = "MATCH (n { id = $id})-[r]-(e) RETURN DISTINCT r";
        // Old Statements below
        private const string kMediaToMediaPathQueryFormat = "MATCH paths = (n)-[*1..5]-(:Media) WHERE \"{0}\" = n.id RETURN DISTINCT paths";
        private const string kNodeAndDirectConnectionsQueryFormat = "MATCH (n) WHERE \"{0}\" = n.id OPTIONAL MATCH (n)-[r]-(e) RETURN n, e, r";

        /// <summary>
        /// Adds the given node to the graph. 
        /// PRECONDITION: The node must be valid
        /// </summary>
        /// <param name="node">The BasicNodeModel of the node to add to the graph</param>
        /// <returns>Returns true if the node was added to the graph successfully</returns>
        public bool AddNode(BasicNodeModel node)
        {
            IStatementResult result;
            using (IDriver driver = GetDatabaseConnection())
            {
                // Create a session
                using (ISession session = driver.Session())
                {
                    // Run the statement
                    // TODO: Implement transactions for safer editing
                    result = session.Run(new Statement(string.Format(kNodeCreationStatement, node.ContentType, node.SerializeToJson())));
                }
            }

            return result.Summary.Counters.NodesCreated > 0;
        }

        public bool UpdateNode(BasicNodeViewModel model)
        {
            throw new NotImplementedException();
        }

        public int AddRelationships(object relationships)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a node and the relationships associated with that node.
        /// </summary>
        /// <param name="id">The GUID of the node to remove</param>
        /// <returns>Returns true if the node was deleted successfully</returns>
        public bool DeleteNode(Guid id)
        {
            IStatementResult result;

            using (IDriver driver = GetDatabaseConnection())
            {
                using (ISession session = driver.Session())
                {
                    // Run the statement
                    result = session.Run(new Statement(kNodeDeleteStatement, new Dictionary<string, object> { { "id", id.ToString() } }));
                }
            }

            return result.Summary.Counters.NodesDeleted > 0;
        }

        public BasicNodeModel GetNode(Guid id)
        {
            IStatementResult result;

            using (IDriver driver = GetDatabaseConnection())
            {
                using (ISession session = driver.Session())
                {
                    result = session.Run(new Statement(kSingleNodeMatchStatement, new Dictionary<string, object> { { "id", id.ToString() } }));
                }
            }

            return BasicNodeModel.FromINode(result.First()[0].As<INode>());
        }

        public IEnumerable<IRelationship> GetNodeRelationships(Guid id)
        {
            IStatementResult result;
            List<IRelationship> relationships = new List<IRelationship>();

            using (IDriver driver = GetDatabaseConnection())
            {
                using (ISession session = driver.Session())
                {
                    result = session.Run(kRelationshipsMatchStatement, new Dictionary<string, object> { { "id", id.ToString() } });
                }
            }

            

            return relationships;
        }

        public IEnumerable<IPath> GetPaths(Guid id)
        {
            // Run the path query
            IStatementResult queryResult = RunQuery(string.Format(kMediaToMediaPathQueryFormat, id));
            // Get the paths record
            IRecord paths = queryResult.Peek();
            // Get the result object
            IEnumerable<IPath> pathsList = paths["paths"].As<List<IPath>>();
            // Consume the result
            queryResult.Consume();
            // Return the result
            return pathsList;
        }

        #region Helper Methods
        /// <summary>
        /// Creates an IDriver to interface with the Neo4j database.
        /// </summary>
        /// <returns>The IDriver to use to interface with the Neo4j database</returns>
        private IDriver GetDatabaseConnection()
        {
            return GraphDatabase.Driver(Properties.Settings.Default.neo4jUrl, AuthTokens.Basic(Properties.Settings.Default.neo4jLogin, Properties.Settings.Default.neo4jPass));
        }
        #endregion

        /// <summary>
        /// Private helper method that runs the given query. 
        /// </summary>
        /// <param name="query">The query string to run</param>
        /// <returns>The result of the query string</returns>
        private IStatementResult RunQuery(string query)
        {
            IStatementResult result = null;

            // Connect to the Neo4j database
            using (IDriver driver = GraphDatabase.Driver(Properties.Settings.Default.neo4jUrl, AuthTokens.Basic(Properties.Settings.Default.neo4jLogin, Properties.Settings.Default.neo4jPass)))
            {
                // Establish a session with the database
                using (ISession session = driver.Session())
                {
                    result = session.Run(query);
                }
            }

            return result;
        }
    }
}