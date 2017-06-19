using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models.Util;

namespace MediaGraph.Models
{

    /// <summary>
    /// Models a relationship object used for graphing.
    /// Contains only the basic information about the relationship.
    /// </summary>
    [JsonObject]
    public class GraphRelationship
    {
        [JsonProperty("start")]
        public Guid StartId { get; set; }
        [JsonProperty("end")]
        public Guid EndId { get; set; }
        [JsonProperty("type")]
        public RelationshipType Label { get; set; }

        public bool EqualConnection(GraphRelationship other)
        {
            return SameConnection(other) && Label == other.Label;
        }

        private bool SameConnection(GraphRelationship other)
        {
            return (StartId == other.StartId && EndId == other.EndId) || (StartId == other.EndId && EndId == other.StartId);
        }
    }

    /// <summary>
    /// Models a relationship object with all of its data.
    /// </summary>
    [JsonObject]
    public class FullRelationship
    {
        [JsonProperty("type")]
        public RelationshipType Label { get; set; }
        [JsonProperty("properties")]
        public KeyValuePair<string, object> Properties { get; set; }
    }
}