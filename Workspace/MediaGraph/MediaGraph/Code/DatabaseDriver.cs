using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4j.Driver.V1;
using MediaGraph.Properties;
using MediaGraph.ViewModels;
using MediaGraph.Models.Util;
using MediaGraph.Models;
using MediaGraph.Code;
using System.Text;
using Newtonsoft.Json;

namespace MediaGraph.Code
{
    public class DatabaseDriver
    {
        public IStatementResult BuildAndRunSearchQuery(SearchViewModel queryData)
        {
            return null;
        }

        /// <summary>
        /// Searches for a node that fits the given information.
        /// </summary>
        /// <param name="queryData">The search to run</param>
        /// <returns>A JSON string representing the node</returns>
        public string GetNode(SearchViewModel queryData)
        {
            IStatementResult queryResult = PerformQuery($"MATCH (node{queryData.Label.ToLabelString()}) WHERE \"{queryData.Title}\" in node.names RETURN node");
            string result = "";

            foreach (IRecord record in queryResult)
            {
                result = GetNodeJSON(record[0].As<INode>());
            }

            return result;
        }

        /// <summary>
        /// Querys the Neo4j database for the paths radiating out from the specified node
        /// </summary>
        /// <param name="queryData">The search query to run</param>
        /// <returns>The JSON string representing the paths radiating out from the specified node</returns>
        public string GetPaths(SearchViewModel queryData)
        {
            IStatementResult queryResult = PerformQuery($"MATCH paths = (start{queryData.Label.ToLabelString()})-[:CAST_IN*1..5]-(:Media) WHERE \"{queryData.Title}\" in start.names RETURN DISTINCT paths");
            List<GraphNode> nodesList = new List<GraphNode>();
            // TODO: Track relationship label - Need to give positions to the graph nodes too
            Dictionary<long, List<long>> relationshipMap = new Dictionary<long, List<long>>();

            // Map results
            foreach(IRecord record in queryResult)
            {
                IPath path = record["paths"].As<IPath>();

                foreach(INode node in path.Nodes)
                {
                    if(node.Labels.ToList().Contains("Media"))
                    {
                        GraphNode gNode = new GraphNode { Id = Guid.Parse(node.Properties["id"].As<string>()), Labels = NodeTypeExtensions.FromNodeLabels(node.Labels), Names = node.Properties["names"].As<List<string>>() };
                        if (nodesList.Find(x => x.Id == gNode.Id) == null)
                            nodesList.Add(gNode);
                    }
                }
                // Map relaionships 
                foreach(IRelationship rel in path.Relationships)
                {
                    // Is the key (start node) present in the map
                    if(relationshipMap.ContainsKey(rel.StartNodeId))
                    {
                        // If the value is not in the list of values
                        if (!relationshipMap[rel.StartNodeId].Contains(rel.EndNodeId))
                            relationshipMap[rel.StartNodeId].Add(rel.EndNodeId);
                    }
                    else
                    {
                        relationshipMap.Add(rel.StartNodeId, new List<long> { rel.EndNodeId });
                    }
                }
            }
            // TODO: Update Reduce

            // Get Media-to-Media relationships - Reduce
            //List<GraphRelationship> relationshipList = new List<GraphRelationship>();
            //foreach(KeyValuePair<Guid, List<Guid>> pair in relationshipMap)
            //{
            //    long currentStart;

            //    while(pair.Value.Count > 1)
            //    {
            //        currentStart = pair.Value[0];
            //        for(int i = 1; i < pair.Value.Count; i++)
            //        {
            //            GraphRelationship graphRel = new GraphRelationship { StartId = Guid.Parse(, EndId = pair.Value[i], Label = RelationshipType.Cast_In };
            //            if (relationshipList.Find(x => x.EqualConnection(graphRel)) == null)
            //                relationshipList.Add(graphRel);
            //        }

            //        pair.Value.RemoveAt(0);
            //    }
            //}

            return JsonConvert.SerializeObject(new { nodes = nodesList, relationships = new List<GraphRelationship>() });
        }
        
        /// <summary>
        /// Private helper method that runs the given query.
        /// Validation and authorization of the user 
        /// </summary>
        /// <param name="query">The query string to run</param>
        /// <returns>The result of the query string</returns>
        private IStatementResult PerformQuery(string query)
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

        /// <summary>
        /// Converts a node into a JSON string.
        /// </summary>
        /// <param name="node">The node for which to get a JSON string</param>
        /// <returns>The JSON string for the node</returns>
        private string GetNodeJSON(INode node)
        {
            return JsonConvert.SerializeObject(new { labels = node.Labels, props = node.Properties });
        }

        /// <summary>
        /// Converts a path object into a JSON string.
        /// </summary>
        /// <param name="path">The path which to convert</param>
        /// <returns>The JSON string for the path</returns>
        private string GetPathJSON(IPath path)
        {
            return JsonConvert.SerializeObject(new { start = path.Start.Id, end = path.End.Id, relationships = path.Relationships});
        }

        /*
         * Important things
         *  - Pathing query: MATCH paths = (start{labels})-[{relationshipLabels}*1..{depth}]-(:Media{mediaEndLabels}) RETURN DISTINCT paths
         *  You can add to array properties using: node.property = node.property + "new element"
         */
    }
}