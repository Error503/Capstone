using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using MediaGraph.Models.Util;
using System.Threading.Tasks;

namespace MediaGraph.ViewModels.Edit
{
    [JsonObject]
    public class NodeDescriptionViewModel : IValidatableObject
    {
        #region Properties
        #region Universal Required Properties
        [JsonProperty("id")]
        public Guid NodeId { get; set; }

        [JsonProperty("type")]
        [Display(Name = "Content Type")]
        public SimpleNodeType SimpleType { get; set; }

        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        [JsonProperty("primaryName")]
        [Display(Name = "Common English Name")]
        public string PrimaryName { get; set; }
        #endregion

        #region Optional Properties
        [JsonProperty("alternateNames")]
        public IEnumerable<string> AlternateNames { get; set; } = null;

        [JsonIgnore]
        [Display(Name = "Other Names")]
        public string AlternateNamesString { get; set; } = null;

        [JsonProperty("franchise", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name = "Franchise")]
        public string Franchise { get; set; } = null;

        [JsonProperty("genre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name = "Genres")]
        public uint Genres { get; set; } = 0;
        #endregion
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Check the trimed primary name string
            if (string.IsNullOrWhiteSpace((PrimaryName = PrimaryName.Trim())))
                errors.Add(new ValidationResult("A name must be provided", new string[] { "PrimaryName" }));
            // If there are alternate names
            if (!string.IsNullOrWhiteSpace(AlternateNamesString))
                // Parse the alternate names
                AlternateNames = ParseAlternateNames();

            // If this node is a media node
            if (((int)SimpleType & (int)NodeType.Generic_Media) == (int)SimpleType)
            {
                // Validate as a media object
                ValidateAsMediaNode(validationContext, ref errors);
            }
            else
            {
                // Give the node a GUID if it needs one
                if (NodeId == Guid.Empty)
                    NodeId = Guid.NewGuid();
            }

            return errors;
        }

        /// <summary>
        /// Helper method that performs validation for media nodes.
        /// </summary>
        /// <param name="context">The validation context</param>
        /// <param name="errors">The collection of validation errors</param>
        private void ValidateAsMediaNode(ValidationContext context, ref List<ValidationResult> errors)
        {
            if (Date == DateTime.MinValue)
                errors.Add(new ValidationResult("Media objects must have a valid release date", new string[] { "Date" }));
            // If a franchise is provided, then it must be a valid franchise
            if (!string.IsNullOrWhiteSpace(Franchise))
            {
                // Trim the Franchise string
                Franchise = Franchise.Trim();
                // Validate the provided franchise as a valid franchise
                // TODO: Franchise validation
            }
            else
            {
                // Ensure that the value of Franchise is null if the value is not valid
                // Protection against input strings such as "     "
                Franchise = null;
            }
        }

        /// <summary>
        /// Helper method that parses the alternate names out of the comma
        /// separated string provided
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> ParseAlternateNames()
        {
            // Split and trim non-empty elements
            return AlternateNamesString.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim());
        }
    }
}