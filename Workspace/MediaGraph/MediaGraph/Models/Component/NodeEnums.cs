using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Models.Component
{
    public enum NodeContentType : byte
    {
        Company = 1,
        Media = 2,
        Person = 3
    }

    public static class NodeContentTypeExtensions
    {
        /// <summary>
        /// Converts the node content type enumeration value into
        /// a string value to be used as the Neo4j query label.
        /// </summary>
        /// <param name="type">The NodeContentType which to convert</param>
        /// <returns>The string representation of the node contentType</returns>
        public static string ToLabelString(this NodeContentType type)
        {
            return Enum.GetName(typeof(NodeContentType), type);
        }
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