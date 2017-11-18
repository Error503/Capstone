using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport
{
    [JsonObject]
    public class BasicNodeModel
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Guid Id { get; set; } = Guid.Empty;
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

        [JsonProperty("relatedCompanies")]
        public List<RelationshipModel> RelatedCompanies { get; set; } = new List<RelationshipModel>();
        [JsonProperty("relatedMedia")]
        public List<RelationshipModel> RelatedMedia { get; set; } = new List<RelationshipModel>();
        [JsonProperty("relatedPeople")]
        public List<RelationshipModel> RelatedPeople { get; set; } = new List<RelationshipModel>();


        public virtual string GetNodeLabels()
        {
            return Enum.GetName(typeof(NodeContentType), ContentType);
        }

        /// <summary>
        /// Generates the properties value for this node.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> GetPropertyMap()
        {
            return new Dictionary<string, object>
            {
                { "id", Id.ToString() },
                { "commonName", CommonName },
                { "othernames", OtherNames },
                { "releaseDate", ReleaseDate?.ToString("yyyy-MM-dd") },
                { "deathDate", DeathDate?.ToString("yyyy-MM-dd") }
            };
        }

        public static BasicNodeModel FromINode(INode node)
        {
            BasicNodeModel result = null;

            try
            {
                NodeContentType contentType = (NodeContentType)Enum.Parse(typeof(NodeContentType), node.Labels[0]);
                if (contentType == NodeContentType.Company)
                {
                    result = new CompanyNodeModel
                    {
                        Id = Guid.Parse(node.Properties["id"].As<string>()),
                        ContentType = contentType,
                        ReleaseDate = node.Properties.ContainsKey("releaseDate") ? DateTime.Parse(node.Properties["releaseDate"].As<string>()) : default(DateTime?),
                        DeathDate = node.Properties.ContainsKey("deathDate") ? DateTime.Parse(node.Properties["deathDate"].As<string>()) : default(DateTime?),
                        CommonName = node.Properties.ContainsKey("commonName") ? node.Properties["commonName"].As<string>() : null,
                        OtherNames = node.Properties.ContainsKey("otherNames") ? node.Properties["otherNames"].As<List<string>>() : new List<string>()
                    };
                }
                else if (contentType == NodeContentType.Media)
                {
                    result = new MediaNodeModel
                    {
                        Id = Guid.Parse(node.Properties["id"].As<string>()),
                        ContentType = contentType,
                        ReleaseDate = node.Properties.ContainsKey("releaseDate") ? DateTime.Parse(node.Properties["releaseDate"].As<string>()) : default(DateTime?),
                        DeathDate = node.Properties.ContainsKey("deathDate") ? DateTime.Parse(node.Properties["deathDate"].As<string>()) : default(DateTime?),
                        CommonName = node.Properties.ContainsKey("commonName") ? node.Properties["commonName"].As<string>() : null,
                        OtherNames = node.Properties.ContainsKey("otherNames") ? node.Properties["otherNames"].As<List<string>>() : new List<string>(),
                        // Media properties
                        MediaType = (NodeMediaType)Enum.Parse(typeof(NodeMediaType), node.Labels[1]),
                        FranchiseName = node.Properties.ContainsKey("franchise") ? node.Properties["franchise"].As<string>() : null,
                        Genres = node.Properties.ContainsKey("genres") ? node.Properties["genres"].As<List<string>>() : new List<string>()
                    };
                }
                else if (contentType == NodeContentType.Person)
                {
                    result = new PersonNodeModel
                    {
                        Id = Guid.Parse(node.Properties["id"].As<string>()),
                        ContentType = contentType,
                        ReleaseDate = node.Properties.ContainsKey("releaseDate") ? DateTime.Parse(node.Properties["releaseDate"].As<string>()) : default(DateTime?),
                        DeathDate = node.Properties.ContainsKey("deathDate") ? DateTime.Parse(node.Properties["deathDate"].As<string>()) : default(DateTime?),
                        CommonName = node.Properties.ContainsKey("commonName") ? node.Properties["commonName"].As<string>() : null,
                        OtherNames = node.Properties.ContainsKey("otherNames") ? node.Properties["otherNames"].As<List<string>>() : new List<string>(),
                        // Person properties
                        FamilyName = node.Properties.ContainsKey("familyName") ? node.Properties["familyName"].As<string>() : null,
                        GivenName = node.Properties.ContainsKey("givenName") ? node.Properties["givenName"].As<string>() : null,
                        Status = node.Properties.ContainsKey("status") ? (PersonStatus)Enum.Parse(typeof(PersonStatus), node.Properties["status"].As<string>()) : 0
                    };
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to parse the given node: (Type: " + node.Labels[0] + " Id: " + node.Properties["id"].As<string>(), e);
            }

            return result;
        }

        /// <summary>
        /// Adds the given relationship to this node.
        /// </summary>
        /// <param name="relationship">The relationship to add</param>
        /// <param name="relatedNode">The related node</param>
        public void AddRelationship(IRelationship relationship, INode relatedNode)
        {
            NodeContentType relatedType;
            Guid relatedId;
            if (Guid.TryParse(relatedNode["id"].As<string>(), out relatedId) && Enum.TryParse<NodeContentType>(relatedNode.Labels[0], out relatedType))
            {
                // Create the relationship
                RelationshipModel relModel = new RelationshipModel
                {
                    SourceId = Id,
                    TargetId = relatedId,
                    TargetName = relatedNode["commonName"].As<string>(),
                    Roles = relationship["roles"].As<List<string>>()
                };

                // Add the relationship to the correct list
                if (relatedType == NodeContentType.Company)
                {
                    RelatedCompanies.Add(relModel);
                }
                else if (relatedType == NodeContentType.Media)
                {
                    RelatedMedia.Add(relModel);
                }
                else if (relatedType == NodeContentType.Person)
                {
                    RelatedPeople.Add(relModel);
                }
            }
        }
    }

    public class CompanyNodeModel : BasicNodeModel
    {
        // This add nothing new
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

        public override string GetNodeLabels()
        {
            return $"{base.GetNodeLabels()}:{Enum.GetName(typeof(NodeMediaType), MediaType)}";
        }

        public override Dictionary<string, object> GetPropertyMap()
        {
            Dictionary<string, object> map = base.GetPropertyMap();
            map.Add("franchiseName", FranchiseName);
            map.Add("genres", Genres);
            return map;
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

        public override Dictionary<string, object> GetPropertyMap()
        {
            Dictionary<string, object> map = base.GetPropertyMap();
            map.Add("givenName", GivenName);
            map.Add("familyName", FamilyName);
            map.Add("status", Status);
            return map;
        }
    }

    [JsonObject]
    public class RelationshipModel
    {
        [JsonProperty("sourceId")]
        public Guid SourceId { get; set; }
        [JsonProperty("targetId")]
        public Guid? TargetId { get; set; }
        [JsonProperty("targetName")]
        public string TargetName { get; set; }
        [JsonProperty("roles")]
        public IEnumerable<string> Roles { get; set; }
    }

    public enum NodeContentType : byte
    {
        Company = 1,
        Media = 2,
        Person = 3
    }

    public enum NodeMediaType : byte
    {
        Audio = 1,
        Book = 2,
        Video = 4,
        Game = 8
    }

    public enum PersonStatus : byte
    {
        Active = 1,
        Inactive = 2,
        Retired = 3,
        Deceased = 4
    }
}
