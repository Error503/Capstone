using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using MediaGraph.Models.Util;

namespace MediaGraph.Models
{
    [JsonObject]
    public class GraphDescriptor
    {
        [JsonProperty("graphStyle")]
        public GraphStyle Style { get; set; }
        [JsonProperty("nodes")]
        public IEnumerable<NodeDefinition> Nodes { get; set; }
        [JsonProperty("relationships")]
        public IEnumerable<RelationshipDefinition> Relationships { get; set; }
    }

    public enum GraphStyle
    {
        Relationship = 0,
        Radial = 1
    }

    [JsonObject]
    public class NodeDefinition
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("position")]
        public GraphPoint Location { get; set; }
        [JsonProperty("label")]
        public NodeType Label { get; set; }
        [JsonProperty("names")]
        public IEnumerable<string> Names { get; set; }
    }

    [JsonObject]
    public class GraphPoint
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
    }

    [JsonObject]
    public class RelationshipDefinition
    {
        [JsonProperty("start")]
        public string StartNode { get; set; }
        [JsonProperty("end")]
        public string EndNode { get; set; }
        [JsonProperty("label")]
        public RelationshipType Label { get; set; }
    }
}