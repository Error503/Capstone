using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Code;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using MediaGraph.Models.Util;

namespace MediaGraph.ViewModels
{
    public class MediaDataViewModel : IValidatableObject
    {
        // TODO: This is currently a model not a view model
        #region Basic Node Data
        public NodeType Label{ get; set; }
        public string Names { get; set; }
        #endregion

        #region Media Only Data
        public DateTime ReleaseDate { get; set; }
        public string Franchise { get; set; }
        public string Genres { get; set; }
        #endregion

        // Relationship data (JSON strings)
        public string RelatedPeople { get; set; }
        public string RelatedCompanies { get; set; }
        public string RelatedMedia { get; set; }

        /// <summary>
        /// Validates the MediaDataViewModel for simple errors such as empty fields
        /// or invalid values.
        /// </summary>
        /// <param name="validationContext">The validation context object that was received</param>
        /// <returns>Returns a collection of the validation errors that are present</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Validate required components
            if (Label == 0)
                errors.Add(new ValidationResult("Invalid label value", new string[] { "Label" }));
            if(string.IsNullOrWhiteSpace(Names))
                errors.Add(new ValidationResult("At least one name must be given.", new string[] { "Names" }));

            // If the label is a media type
            if((NodeType.Generic_Media & Label) == Label)
            {
                // Allow ReleaseDate and Franchise to be null, they may not be known
                // Validate genres
                if (string.IsNullOrWhiteSpace(Genres))
                    errors.Add(new ValidationResult("At least one genre must be given for media objects.", new string[] { "Genres" }));
            }

            List<int> badRelationships;
            // Validate the JSON relationship objects
            if (!AreRelationshipsValid(RelatedPeople, out badRelationships))
                errors.Add(new ValidationResult($"Invalid relationships in 'Related People' entries ({badRelationships.AsString()})", new string[] { "RelatedPeople" }));
            if (!AreRelationshipsValid(RelatedCompanies, out badRelationships))
                errors.Add(new ValidationResult($"Invalid relationships in 'Related Companies' entries ({badRelationships.AsString()})", new string[] { "RelatedCompanies" }));
            if (!AreRelationshipsValid(RelatedMedia, out badRelationships))
                errors.Add(new ValidationResult($"Invalid relationships in 'Related Media' entries ({badRelationships.AsString()})", new string[] { "RelatedMedia" }));

            return errors;
        }

        /// <summary>
        /// Validates the given string as a relationship defining JSON object.
        /// </summary>
        /// <param name="jsonString">The JSON string to validate</param>
        /// <param name="badRelationships">An array of ints returned to indicate the invalid relationships</param>
        /// <returns>Returns true if the relationship object is valid</returns>
        private bool AreRelationshipsValid(string jsonString, out List<int> badRelationships)
        {
            bool isValid = true;
            badRelationships = new List<int>();
            // Create the JSON object from the string
            JObject jsonObject = JObject.Parse(jsonString);
            List<JToken> entryTokens = jsonObject.GetValue("entries").ToList();

            for(int i = 0; i < entryTokens.Count; i++)
            {
                // TODO: May want to use regex instead of just null or whitespace checks
                // If the relationships is invalid
                if(string.IsNullOrWhiteSpace(entryTokens[i].Value<string>("name")) || string.IsNullOrWhiteSpace(entryTokens[i].Value<string>("roles")))
                {
                    // Set is valid to false
                    isValid = false;
                    // Add the index to the bad relationships list
                    badRelationships.Add(i);
                }
            }

            return isValid;
        }
    }
}