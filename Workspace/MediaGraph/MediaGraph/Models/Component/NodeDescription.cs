using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;
using MediaGraph.Models.Util;
using MediaGraph.ViewModels.Edit;
using System.Text;
using Neo4j.Driver.V1;

namespace MediaGraph.Models.Component
{
    [JsonObject]
    public class NodeDescription
    {
        #region Properties 
        [JsonProperty("id")]
        public Guid NodeId { get; set; }
        [JsonProperty("type")]
        public NodeType ContentType { get; set; }
        [JsonProperty("primaryName")]
        public string PrimaryName { get; set; }

        // Optional Properties
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Date { get; set; }
        [JsonProperty("otherNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<string> OtherNames { get; set; } = null;
        [JsonProperty("franchise", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Franchise { get; set; } = null;
        [JsonProperty("genres", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Genres { get; set; } = 0;
        #endregion

        public string LabelString
        {
            get { return ContentType.ToLabelString(); }
        }

        /// <summary>
        /// Creates a NodeDescription from the given INode.
        /// </summary>
        /// <param name="node">The INode from which to generate the NodeDescription</param>
        /// <returns>The NodeDescription generated from the given INode</returns>
        public static NodeDescription FromINode(INode node)
        {
            return new NodeDescription
            {
                NodeId = new Guid(node["id"].As<string>()),
                ContentType = NodeTypeExtensions.FromNodeLabels(node.Labels),
                PrimaryName = node["primaryName"].As<string>(),
                Date = DateTime.Parse(node["date"].As<string>()),
                OtherNames = node["otherNames"].As<List<string>>(),
                Franchise = node["franchise"].As<string>(),
                Genres = node["genres"].As<uint>()
            };
        }
        
        /// <summary>
        /// Creates a view model representation of this node description.
        /// </summary>
        /// <returns></returns>
        public NodeDescriptionViewModel ToViewModel()
        {
            return new NodeDescriptionViewModel
            {
                NodeId = NodeId,
                ContentType = ContentType,
                PrimaryName = PrimaryName,
                Date = Date,
                AlternateNamesString = GetAlternateNamesString(OtherNames),
                Franchise = Franchise,
                Genres = Genres
            };
        }

        /// <summary>
        /// Creates a NodeDescription from the given view model.
        /// </summary>
        /// <param name="viewModel">The NodeDescriptionViewModel to use to create a NodeDescription</param>
        public static NodeDescription FromViewModel(NodeDescriptionViewModel viewModel)
        {
            return new NodeDescription
            {
                NodeId = viewModel.NodeId,
                ContentType = viewModel.ContentType,
                PrimaryName = viewModel.PrimaryName,
                Date = viewModel.Date,
                OtherNames = ParseAlternateNames(viewModel.AlternateNamesString),
                Franchise = viewModel.Franchise,
                Genres = viewModel.Genres
            };
        }

        /// <summary>
        /// Helper method that parses the alternate names into a collection.
        /// </summary>
        /// <param name="namesString">The string to parse</param>
        /// <returns></returns>
        private static IEnumerable<string> ParseAlternateNames(string namesString)
        {
            return namesString.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
        }

        /// <summary>
        /// Helper method that converts the collection of alternate names into a
        /// a single, comma separated string.
        /// </summary>
        /// <param name="alternateNames">The collection of alternate names</param>
        /// <returns>A comma separated string representation of the alternate names</returns>
        private static string GetAlternateNamesString(IEnumerable<string> alternateNames)
        {
            StringBuilder builder = new StringBuilder();

            // Build the string
            foreach(string name in alternateNames)
            {
                builder.AppendFormat("{0},", name);
            }

            // Remove the ending comma and return the string
            return builder.Remove(builder.Length - 1, 1).ToString();
        }

        /// <summary>
        /// Generates a dictionary containing the node's data
        /// </summary>
        /// <returns>A collection of key value pairs containing the node's data</returns>
        public Dictionary<string, object> GetParameterMap()
        {
            return new Dictionary<string, object>
            {
                { "id", NodeId.ToString() },
                { "primaryName", PrimaryName },
                { "date", Date.As<DateTime>().ToString() },
                { "otherNames", OtherNames },
                { "franchise", Franchise },
                { "genres", Genres }
            };
        }
    }
}