using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Models.Component
{
    [JsonObject]
    public class BasicNodeModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("contentType")]
        public NodeContentType ContentType { get; set; }
        [JsonProperty("commonName")]
        public string CommonName { get; set; }
        [JsonProperty("otherNames")]
        public IEnumerable<string> OtherNames { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; }
        [JsonProperty("deathDate")]
        public DateTime? DeathDate { get; set; }
        //[JsonProperty("links")]
        //public Dictionary<string, string> Links { get; set; }

        /// <summary>
        /// Returns the string represtation of this node's labels.
        /// </summary>
        /// <returns>The string represenation of this node's labels</returns>
        public virtual string GetNodeLabels()
        {
            return ContentType.ToString();
        }

        /// <summary>
        /// Serializes the node to a JSON object to be put into the database.
        /// </summary>
        /// <returns>The JSON string of this node</returns>
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Dserializes the given string to the provided type
        /// </summary>
        /// <typeparam name="T">The BasicNodeType to which to deserialize</typeparam>
        /// <param name="json">The JSON string</param>
        /// <returns>A node of the specified type parsed from the given string</returns>
        public static T DeserializeAsType<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static BasicNodeModel FromINode(INode node)
        {
            BasicNodeModel result = null;
            NodeContentType contentType = (NodeContentType)node.Properties["contentType"].As<int>();

            // TODO: Parsing of the Neo4j model

            return result;
        }
    }

    // CompanyNodeModel adds nothing to the basic model
    [JsonObject]
    public class CompanyNodeModel : BasicNodeModel
    {

    }

    [JsonObject]
    public class MediaNodeModel : BasicNodeModel
    {
        [JsonProperty("mediaType")]
        public NodeMediaType MediaType { get; set; }
        [JsonProperty("franchiseName")]
        public string FranchiseName { get; set; }
        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }
        //[JsonProperty("regionalReleaseDates")]
        //public Dictionary<string, DateTime> RegionalReleaseDates { get; set; }
        //[JsonProperty("platforms")]
        //public IEnumerable<string> Platforms { get; set; }

        /// <summary>
        /// Overrides the GetNodeLabels method to add the media type to the label.
        /// </summary>
        /// <returns>The string representation of this node's label</returns>
        public override string GetNodeLabels()
        {
            return $"{base.GetNodeLabels()}:{MediaType.ToString()}";
        }
    }

    [JsonObject]
    public class PersonNodeModel : BasicNodeModel
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }
        [JsonProperty("familyName")]
        public string FamilyName { get; set; }
        [JsonProperty("status")]
        public PersonStatus Status { get; set; }
        //[JsonProperty("nationality")]
        //public string Nationality { get; set; }
    }
}