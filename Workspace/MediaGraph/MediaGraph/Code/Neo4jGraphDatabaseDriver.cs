using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models;
using Neo4j.Driver.V1;
using MediaGraph.Models.Util;

namespace MediaGraph.Code
{
    public class Neo4jGraphDatabaseDriver : IGraphDatabaseDriver
    {
        private const string kSingleMatchQueryFormat = "MATCH (n) WHERE \"{0}\" = n.id RETURN n";
        private const string kDeleteQueryFormat = "MATCH (n) WHERE \"{0}\" = n.id DETACH DELETE n";
        private const string kMediaToMediaPathQueryFormat = "MATCH paths = (n)-[*1..5]-(:Media) WHERE \"{0}\" = n.id RETURN DISTINCT paths";
        private const string kNodeAndDirectConnectionsQueryFormat = "MATCH (n) WHERE \"{0}\" = n.id OPTIONAL MATCH (n)-[r]-(e) RETURN n, e, r";

        public bool AddNode(EditNode node)
        {
            throw new NotImplementedException();
        }

        public bool DeleteNode(Guid id)
        {
            // Run the delete query 
            IStatementResult queryResult = RunQuery(string.Format(kDeleteQueryFormat, id));
            // If at least one node was deleted, then deletion was completed successfully
            return queryResult.Summary.Counters.NodesDeleted > 0;
        }

        public INode GetNode(Guid id)
        {
            INode result = null;
            IStatementResult queryResult = RunQuery(string.Format(kSingleMatchQueryFormat, id));
            // Get the first (and only record)
            IRecord record = queryResult.Peek();
            // If the record exists, then get the value of the node
            result = record?["n"].As<INode>();
            // Consume the result
            queryResult.Consume();
            // Return the result
            return result;
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

        public EditNode GetNodeForEdits(Guid id)
        {
            return null;
        }

        public bool UpdateNode(FullNode node)
        {
            throw new NotImplementedException();
        }

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