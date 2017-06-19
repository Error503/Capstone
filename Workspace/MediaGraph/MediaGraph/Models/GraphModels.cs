using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Util;

namespace MediaGraph.Models
{
    [JsonObject]
    public class GraphModel
    {
        [JsonProperty("type")]
        public DisplayType Style { get; set; }
        [JsonProperty("nodes")]
        public IEnumerable<GraphNode> Nodes { get; set; }
        [JsonProperty("relationships")]
        public IEnumerable<GraphRelationship> Relationships { get; set; }
    }
}