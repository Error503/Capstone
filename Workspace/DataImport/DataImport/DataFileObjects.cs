using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataImport
{
    /// <summary>
    /// The object representation of a data file and its contents.
    /// </summary>
    [XmlRoot("Datafile")]
    public class DataFileObject
    {
        [XmlArray("MediaObjects")]
        [XmlArrayItem("Media")]
        public List<MediaNode> MediaObjects { get; set; }
        [XmlArray("People")]
        [XmlArrayItem("Person")]
        public List<NodeBase> People { get; set; }
        [XmlArray("Companies")]
        [XmlArrayItem("Company")]
        public List<NodeBase> Companies { get; set; }
        [XmlArray("Relationships")]
        [XmlArrayItem("Relationship")]
        public List<Relationship> Relationships { get; set; }


        /// <summary>
        /// Generates the Cypher node strings for the media nodes
        /// in the data file.
        /// </summary>
        /// <returns>A list of strings representing the media node objects
        /// for use in a Cypher query</returns>
        public List<string> GetMediaCypherStrings()
        {
            string creationFormat = "({0}:Media:{1} {2})";
            List<string> cypherStrings = new List<string>();

            // Loop through the list of media objects
            foreach(MediaNode node in MediaObjects)
            {
                cypherStrings.Add(string.Format(creationFormat, node.Id, node.Type, ("{" + node.GetCypherPropertyString() + "}")));
            }

            return cypherStrings;
        }


        /// <summary>
        /// Generates the Cypher node strings for the people nodes
        /// in the data file.
        /// </summary>
        /// <returns>A list of strings representing the people node objects
        /// for use in a Cypher query</returns>
        public List<string> GetPeopleCypherStrings()
        {
            string creationFormat = "({0}:Person {1})";
            List<string> cypherStrings = new List<string>();

            // Loop through the list of people
            foreach(NodeBase node in People)
            {
                cypherStrings.Add(string.Format(creationFormat, node.Id, ("{" + node.GetCypherPropertyString() + "}")));
            }

            return cypherStrings;
        }

        /// <summary>
        /// Generates the Cypher node strings for the companies defined in the data file.
        /// </summary>
        /// <returns>A list of strings representing the company node objects
        /// for use in a Cypher query</returns>
        public List<string> GetCompaniesCypherStrings()
        {
            string creationFormat = "({0}:Company {1})";
            List<string> cypherStrings = new List<string>();

            // Loop through the list of companies
            foreach(NodeBase node in Companies)
            {
                cypherStrings.Add(string.Format(creationFormat, node.Id, ("{" + node.GetCypherPropertyString() + "}")));
            }

            return cypherStrings;
        }

        /// <summary>
        /// Generates the Cypher relationship strings defined in the data file.
        /// </summary>
        /// <returns>A list containing the Cypher relationship strings defined
        /// in the data file</returns>
        public List<string> GetRelationshipStrings()
        {
            List<string> relationshipStrings = new List<string>();

            // Loop through the relationships
            foreach(Relationship rel in Relationships)
            {
                relationshipStrings.Add(rel.GetCypherRelationshipString());
            }

            return relationshipStrings;
        }
    }

    /// <summary>
    /// The base class for all top level node objects.
    /// </summary>
    public class NodeBase
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlArray("Names")]
        [XmlArrayItem("item")]
        public List<string> Names { get; set; }

        /// <summary>
        /// Generates and returns the Cyper property string for the base
        /// node. The returned string is not formated with brackets '{'.
        /// </summary>
        /// <returns>A string representing the properties of the node
        /// for use in cypher queries</returns>
        public virtual string GetCypherPropertyString()
        {
            return $"names:{CypherQueryUtils.GetCypherStringArray(Names)}";
        }
    }

    #region Top Level Objects
    /// <summary>
    /// Top level object for Media objects such as video games, 
    /// movies, shows, books, and music.
    /// </summary>
    public class MediaNode : NodeBase
    {
        [XmlAttribute("type")]
        public MediaType Type { get; set; }

        [XmlElement("ReleaseDate")]
        public DateTime ReleaseDate { get; set; }

        [XmlElement("Franchise")]
        public string Franchise { get; set; }

        [XmlArray("Genres")]
        [XmlArrayItem("item")]
        public List<String> Genres { get; set; }

        /// <summary>
        /// Generates the Cypher property string for the MediaNode.
        /// </summary>
        /// <returns>The Cypher property string for the MediaNode</returns>
        public override string GetCypherPropertyString()
        {
            // Get the base string
            return $"{base.GetCypherPropertyString()}, releaseDate:\"{ReleaseDate.ToShortDateString()}\", franchise:\"{Franchise}\", genres:{CypherQueryUtils.GetCypherStringArray(Genres)}";
        }
    }
    #endregion

    /// <summary>
    /// Defines a relationship between two two level nodes.
    /// </summary>
    public class Relationship
    {

        [XmlAttribute("source")]
        public string SourceId { get; set; }

        [XmlAttribute("target")]
        public string TargetId { get; set; }

        [XmlAttribute("type")]
        public RelationshipType Type { get; set; }

        [XmlArray("Roles", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<string> Roles { get; set; }

        /// <summary>
        /// Generates the relationship string for the relationship object.
        /// </summary>
        /// <returns>The Cypher string representing the relationship object</returns>
        public string GetCypherRelationshipString()
        {
            SourceId = SourceId.Split(':')[1];
            TargetId = TargetId.Split(':')[1];

            string rolesPropertyString = "{roles:" + CypherQueryUtils.GetCypherStringArray(Roles) + "}";
            return $"({SourceId})-[:{CypherQueryUtils.GetCypherRelationshipType(Type)} {rolesPropertyString}]-({TargetId})";
        }
    }

    public enum MediaType : byte
    {
        /// <summary>
        /// Any video game.
        /// </summary>
        Game = 0,
        /// <summary>
        /// Any television show.
        /// </summary>
        Show = 1,
        /// <summary>
        /// Any movie.
        /// </summary>
        Movie = 2,
        /// <summary>
        /// Any form of book.
        /// </summary>
        Book = 3
    }

    public enum RelationshipType : byte
    {
        /// <summary>
        /// Company-to-Media relationship
        /// Represents a company that was involved in a media's production.
        /// </summary>
        Created = 0,
        /// <summary>
        /// Person-to-Media relationship.
        /// Represents a person who is cast in a media.
        /// </summary>
        Cast = 1,
        // Media-to-Media relationships
        Sequel = 2,
        Prequel = 3,
        Spinoff = 4,
        Adaptation = 5,
        Remake = 6,
        Series = 7
    }
}
