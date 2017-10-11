﻿using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public List<RelationshipModel> RelatedCompanies { get; set; } = new List<RelationshipModel>();
        public List<RelationshipModel> RelatedMedia { get; set; } = new List<RelationshipModel>();
        public List<RelationshipModel> RelatedPeople { get; set; } = new List<RelationshipModel>();

        /// <summary>
        /// Returns the string represtation of this node's labels.
        /// </summary>
        /// <returns>The string represenation of this node's labels</returns>
        public virtual string GetNodeLabels()
        {
            return Enum.GetName(typeof(NodeContentType), ContentType);
        }

        /// <summary>
        /// Creates and returns the property map for the node
        /// </summary>
        /// <returns>The property map for the node</returns>
        public virtual Dictionary<string, object> GetPropertyMap()
        {
            return new Dictionary<string, object>
            {
                { "id", Id.ToString() },
                { "commonName", CommonName },
                { "otherNames", OtherNames },
                { "releaseDate", ReleaseDate?.ToShortDateString() },
                { "deathDate", DeathDate?.ToShortDateString() }
            };
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
        /// Creates a BasicNodeModel based on the given INode.
        /// </summary>
        /// <exception cref="ArgumentException">If the INode could not be parsed</exception>
        /// <param name="node">The INode from which to create the BasicNodeModel</param>
        /// <returns>The created BasicNodeModel or null if an error occurred</returns>
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
            if(Guid.TryParse(relatedNode["id"].As<string>(), out relatedId) && Enum.TryParse<NodeContentType>(relatedNode.Labels[0], out relatedType))
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
            return $"{base.GetNodeLabels()}:{Enum.GetName(typeof(NodeMediaType), MediaType)}";
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

    public class RelationshipModel
    {
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public string TargetName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}