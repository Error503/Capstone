using Neo4j.Driver.V1;
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
        public long? ReleaseDate { get; set; }
        [JsonProperty("deathDate")]
        public long? DeathDate { get; set; }
        //[JsonProperty("links")]
        //public Dictionary<string, string> Links { get; set; }

        public List<RelationshipModel> Relationships { get; set; }

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
                { "releaseDate", ReleaseDate },
                { "deathDate", DeathDate }
            };
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
                    result = new CompanyNodeModel();
                }
                else if (contentType == NodeContentType.Media)
                {
                    result = new MediaNodeModel
                    {
                        // Media properties
                        MediaType = node.Labels.Count > 2 ? (NodeMediaType)Enum.Parse(typeof(NodeMediaType), node.Labels[1]) : 0,
                        Franchise = node.Properties.ContainsKey("franchise") ? node.Properties["franchise"].As<string>() : null,
                        Genres = node.Properties.ContainsKey("genres") ? node.Properties["genres"].As<List<string>>() : new List<string>()
                    };
                }
                else if (contentType == NodeContentType.Person)
                {
                    result = new PersonNodeModel
                    {
                        // Person properties
                        FamilyName = node.Properties.ContainsKey("familyName") ? node.Properties["familyName"].As<string>() : null,
                        GivenName = node.Properties.ContainsKey("givenName") ? node.Properties["givenName"].As<string>() : null,
                        Status = node.Properties.ContainsKey("status") ? (PersonStatus)Enum.Parse(typeof(PersonStatus), node.Properties["status"].As<string>()) : 0
                    };

                    // If the family and given name was not populated and there is a common name,
                    if(((PersonNodeModel)result).FamilyName == null && ((PersonNodeModel)result).GivenName == null &&
                        node.Properties.ContainsKey("commonName"))
                    {
                        // Parse the names out of the common name
                        string[] nameParts = node.Properties["commonName"].As<string>().Split(' ');
                        string givenName = "";
                        string familyName = "";
                        for(int i = 0; i < nameParts.Length; i++)
                        {
                            if(i != nameParts.Length - 1)
                            {
                                givenName += $"{nameParts[i]} ";
                            }
                            else
                            {
                                familyName = nameParts[i];
                            }
                        }

                        ((PersonNodeModel)result).GivenName = givenName.Trim();
                        ((PersonNodeModel)result).FamilyName = familyName;
                    }
                }
                // Basic properties
                result.Id = Guid.Parse(node.Properties["id"].As<string>());
                result.ContentType = contentType;
                result.ReleaseDate = node.Properties.ContainsKey("releaseDate") ? node.Properties["releaseDate"].As<long>() : default(long?);
                result.DeathDate = node.Properties.ContainsKey("deathDate") ? node.Properties["deathDate"].As<long>() : default(long?);
                result.CommonName = node.Properties.ContainsKey("commonName") ? node.Properties["commonName"].As<string>() : null;
                result.OtherNames = node.Properties.ContainsKey("otherNames") ? node.Properties["otherNames"].As<List<string>>() : new List<string>();
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
                    TargetType = relatedType,
                    TargetName = relatedNode["commonName"].As<string>(),
                    Roles = relationship["roles"].As<List<string>>()
                };
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
        [JsonProperty("franchise")]
        public string Franchise { get; set; }
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
        public Guid? TargetId { get; set; }
        public NodeContentType TargetType { get; set; }
        public string TargetName { get; set; }
        public IEnumerable<string> Roles { get; set; }

        /// <summary>
        /// Generates the label string for this relationship.
        /// This is the same as calling GetNodeLabels on node with only one label.
        /// </summary>
        /// <returns>The label for this relationship</returns>
        public string GetNodeLabel()
        {
            if (TargetType == 0)
                throw new ArgumentException("TargetType is invalid!!!", "TargetType");
            return Enum.GetName(typeof(NodeContentType), TargetType);
        }
    }
}