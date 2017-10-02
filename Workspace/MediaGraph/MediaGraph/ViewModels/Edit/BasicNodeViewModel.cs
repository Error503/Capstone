using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Text;

namespace MediaGraph.ViewModels.Edit
{

    // TODO: Server-side validation of node and relationship view models

    [JsonObject]
    public class BasicNodeViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("nodeType")]
        public string NodeType { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; } // Also "date of birth" or "date of founding"

        [JsonProperty("commonName")]
        public string CommonName { get; set; }
        [JsonProperty("otherNames")]
        public IEnumerable<string> OtherNames { get; set; }

        // NOTE: When adding to the Neo4j database, use NodeType in this class to determine relationship direction

        [JsonProperty("relatedCompanies")]
        public IEnumerable<RelationshipViewModel> RelatedCompanies { get; set; }
        [JsonProperty("relatedMedia")]
        public IEnumerable<RelationshipViewModel> RelatedMedia { get; set; }
        [JsonProperty("relatedPeople")]
        public IEnumerable<RelationshipViewModel> RelatedPeople { get; set; }

        // Other possible properties
        //[JsonProperty("links")]
        //public Dictionary<WebsiteKey, string> AdditionalLinks { get; set; }
    }

    [JsonObject]
    public class MediaNodeViewModel : BasicNodeViewModel
    {
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
        [JsonProperty("franchiseName")]
        public string FranchiseName { get; set; }
        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        // Other possible properties
        //public Dictionary<string, DateTime> RegionalReleaseDates { get; set; } // string key could be replaced with Region enum
        //public IEnumerable<string> Platforms { get; set; } // string could be replaced with a Platforms enum
    }

    [JsonObject]
    public class PersonNodeViewModel : BasicNodeViewModel
    {
        // Other possible properties
        [JsonProperty("givenName")]
        public string GivenName { get; set; } 
        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        // public string Nationality { get; set; } // string could be replaced with a Nationalities enum? - might be too large
        // public PersonStatus Status { get; set; } // Status of the person in their career - enum of Active, Inactive, Retired, Deceased
        // public DateTime? DateOfDeath { get; set; }
    }

    [JsonObject]
    public class CompanyNodeViewModel : BasicNodeViewModel
    {
        // Other possible properties
        public DateTime? DateOfDeath { get; set; } // Date that the company ended
    }

    [JsonObject]
    public class RelationshipViewModel
    {
        [JsonProperty("sourceId")]
        public Guid SourceId { get; set; }
        [JsonProperty("targetId")]
        public Guid? TargetId { get; set; }
        [JsonProperty("targetName")]
        public string TargetName { get; set; }
        [JsonProperty("roles")]
        public IEnumerable<string> Roles { get; set; }

        /// <summary>
        /// Converts the roles collection into a string.
        /// </summary>
        /// <returns>A string representation of the roles collection</returns>
        public string GetRolesString()
        {
            StringBuilder builder = new StringBuilder();

            foreach(string r in Roles)
            {
                builder.AppendFormat("{0}, ", r);
            }

            // Remove the trailing space and comma before returning
            return builder.ToString().TrimEnd(' ', ',');
        }
    }
}