using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Util;
using Neo4j.Driver.V1;

namespace MediaGraph.Models
{
    /// <summary>
    /// Models a node object for the relational graph display.
    /// </summary>
    [JsonObject]
    public class GraphNode
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("type")]
        public NodeType Labels { get; set; }
        [JsonProperty("names")]
        public IEnumerable<string> Names { get; set; }
    }

    /// <summary>
    /// Models a node with all of it's data.
    /// </summary>
    [JsonObject]
    public class FullNode
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("type")]
        public NodeType Labels { get; set; }
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }    

        /// <summary>
        /// Creates a FullNode from the given INode object.
        /// </summary>
        /// <param name="node">The INode object to turn into a FullNode</param>
        /// <returns>The FullNode object created from the INode</returns>
        public static FullNode FromINode(INode node)
        {
            return new FullNode { Id = Guid.Parse(node.Properties["id"].As<string>()), Labels = NodeTypeExtensions.FromNodeLabels(node.Labels), Properties = GetPropertyDictionary(node.Properties) };
        }

        /// <summary>
        /// Converts the node properites into a dictionary.
        /// </summary>
        /// <param name="nodeProperties">The ReadOnlyDictionary containing the properties</param>
        /// <returns>The created dictionary</returns>
        private static Dictionary<string, object> GetPropertyDictionary(IReadOnlyDictionary<string, object> nodeProperties)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> pair in nodeProperties)
            {
                // Skip over the Id property
                if(pair.Key != "id")
                    dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }
    }

    /// <summary>
    /// Models a single node and the relationships to the nodes directly around it.
    /// This is the model that is used for added and editing nodes
    /// </summary>
    public class EditNode
    {
        public FullNode NodeData { get; set; }
        public IDictionary<GuidStringKey, RelationshipType> DirectRelationships { get; set; }

        /// <summary>
        /// Gets the creation string for the edit node.
        /// This is the string that will be used to create the node.
        /// </summary>
        /// <returns>The query string that can be used to create the node</returns>
        public string GetCreationString()
        {
            return "";
        }
    }
}