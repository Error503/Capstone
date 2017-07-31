using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Util;

namespace MediaGraph.Code
{
    /// <summary>
    /// Static class with method used for generating Cypher queries.
    /// </summary>
    public static class CypherQueryBuilder
    {
        /// <summary>
        /// Builds a Cypher query with basic information information
        /// </summary>
        /// <param name="startNodeLabel">The labels on the starting node</param>
        /// <param name="startNodeName">The name of the starting node</param>
        /// <param name="endNodeLabel">The labels for the end node(s)</param>
        /// <param name="endNodeName">An optional name for the end node</param>
        /// <param name="search">An optional search type to use for the query</param>
        /// <returns>A string representing the built cypher query</returns>
        public static string BuildQuery(NodeType startNodeLabel, string startNodeName, NodeType endNodeLabel, string endNodeName = "", SearchType search = SearchType.DirectRelationships)
        {
            // TODO: Queries
            // Return a simple query for now
            return $"MATCH p = (start:Media:Game)-[*1..2]-(:Media) WHERE \"Hyperdimension Neptunia Re;birth1\" in start.names RETURN DISTINCT p";
        }

        /// <summary>
        /// Returns a query string for getting the data of a specific node.
        /// </summary>
        /// <param name="title">The title, or name, on the node</param>
        /// <param name="type">The label of the node</param>
        /// <returns>The query string to return the data for the specified node</returns>
        public static string NodeDataQuery(string title, NodeType type)
        {
            return $"MATCH (n{type.ToLabelString()}) WHERE \"{title}\" IN n.names RETURN n";
        }

        public static string BuildQuery()
        {
            return null;
            /*
             * Query components:
             *  - What is the type of our starting node?
             *      Person, Media(Game, Show, Book, etc.), Company
             *  - Information to identify the starting node
             *      Name(s), genre(s), release date, franchise
             *  - What information do we want back?
             *      Single node, related nodes, similar nodes (franchise relationship)
             *  - If looking for related nodes, what relationship constraints do we have?
             *      Relationship label(s) and max depth
             *  - If looking for related nodes, what end node constraints do we have?
             *      node label(s), property information
             *  - If constraining the ending nodes by property information:
             *      Are we looking for shortest paths? Are we just looking for those nodes?
             */
        }
    }

    public class QueryRequest
    {
        public QueryType Type { get; set; }

        // Starting node(s) filtering information
        public NodeType StartingNodeLabels { get; set; }
        public Dictionary<string, string> StartingNodePropertyFilters { get; set; }

        // "Hyperdimension Neptunia Re;birth1" 
        // Probably will only want to store lowercased values in the database - can format it on return
        // Use a comma separated list, if title contains a comma "title with,comma"

        // Relationship filtering information
        //public RelationshipLabel RelationshipLabels { get; set; }
        public Dictionary<string, string> RelationshipPropertyFilters { get; set; }

        // Ending node(s) filtering information
        public NodeType EndingNodeLabels { get; set; }
        public Dictionary<string, string> EndingNodePropertyFilters { get; set; }
    }

    public enum SearchType : byte
    {
        SingleNode = 1,
        DirectRelationships = 2,
        RelationshipGraph = 3,
        ShortestPath = 4
    }

    public enum QueryType : byte
    {
        /// <summary>
        /// Returns information for a single node or set of nodes.
        /// </summary>
        SingleNodeSearch = 0,
        /// <summary>
        /// Direct relationships from a single node or set of nodes.
        /// Most useful for displaying direct Company-to-Media and
        /// Person-to-Media relationships.
        /// </summary>
        DirectRelationships = 1,
        /// <summary>
        /// Normal search for finding related nodes to a certain
        /// depth and/or number of nodes.
        /// </summary>
        DepthRelationships = 2,
        /// <summary>
        /// Search for the shortest path between two nodes.
        /// </summary>
        ShortestPathSearch = 3
    }
}