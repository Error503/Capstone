using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models;
using MediaGraph.Models.Util;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MediaGraph.ViewModels
{
    [JsonObject]
    public class NodeViewModel : IValidatableObject
    {
        [JsonProperty("node")]
        public NodeDataViewModel Node { get; set; }
        [JsonProperty("relationships")]
        public RelationshipsViewModel Relationships { get; set; }

        /// <summary>
        /// Validates the node view model
        /// </summary>
        /// <param name="validationContext">The current validation context</param>
        /// <returns>A collection of validation errors</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Node content should never be null, if it is then the code has a problem
            if (Node == null)
                errors.Add(new ValidationResult("Binding issue - Node data is null"));
            else
                errors.AddRange(Node.Validate(validationContext));

            // Relationships is allowed to be null in the case that no relationships are specified.
            // Validate the relationships if they are present
            if (Relationships != null)
                errors.AddRange(Relationships.Validate(validationContext));
            return errors;
        }
    }

    [JsonObject]
    public class NodeDataViewModel
    {
        [JsonProperty("id")]
        public Guid? Id { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("commonName")]
        public string CommonName { get; set; }
        [JsonProperty("otherNames")]
        public IEnumerable<string> OtherNames { get; set; }
        [JsonProperty("date")]
        public DateTime? Date { get; set; }
        [JsonProperty("mediaData")]
        public MediaDataViewModel MediaData { get; set; } = null;

        /// <summary>
        /// Creates a single string for each of the names in the other names collection.
        /// </summary>
        /// <returns>A single string containing all of the alternate names</returns>
        public string GetOtherNamesString()
        {
            StringBuilder builder = new StringBuilder();

            foreach(string n in OtherNames)
            {
                builder.AppendFormat("{0}, ", n);
            }

            // Remove the trailing space and comma before returning
            return builder.ToString().TrimEnd(' ', ',');
        }

        /// <summary>
        /// Validates this NodeDataViewModel.
        /// </summary>
        /// <param name="validationContext">The current validation context</param>
        /// <returns>A collection of errors</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // A content type must be provided,
            if (string.IsNullOrEmpty(ContentType))
                errors.Add(new ValidationResult("A content type must be provided", new string[] { "ContentType" }));

            // A name must be provided,
            if(string.IsNullOrEmpty(CommonName))
                errors.Add(new ValidationResult("A name must be provided", new string[] { "commonName" }));

            // If the node is a media node, then validate the media data
            if (ContentType == "media" && MediaData != null)
                errors.AddRange(MediaData.Validate(validationContext));

            return errors;
        }
    }

    [JsonObject]
    public class MediaDataViewModel
    {
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
        [JsonProperty("franchiseName")]
        public string FranchiseName { get; set; }
        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        /// <summary>
        /// Returns the genres as a single string.
        /// </summary>
        /// <returns>The genres of the media as a string</returns>
        public string GetGenresString()
        {
            StringBuilder builder = new StringBuilder();

            foreach(string g in Genres)
            {
                builder.AppendFormat("{0}, ", g);
            }

            // Remove the trailing space and comma before returning
            return builder.ToString().TrimEnd(' ', ',');
        }

        public List<ValidationResult> Validate(ValidationContext context)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // A media type must be provided
            if (string.IsNullOrWhiteSpace("MediaType"))
                errors.Add(new ValidationResult("A media type must be provided.", new string[] { "MediaType" }));

            // Franchise and genres may be left out - for cases of unknown genre or stand alone titles

            return errors;
        }
    }

    [JsonObject]
    public class RelationshipsViewModel
    {
        [JsonProperty("companies")]
        public IEnumerable<RelationshipViewModel> Companies { get; set; }
        [JsonProperty("people")]
        public IEnumerable<RelationshipViewModel> People { get; set; }
        [JsonProperty("media")]
        public IEnumerable<RelationshipViewModel> Media { get; set; }

        /// <summary>
        /// Validates this relationships object.
        /// </summary>
        /// <param name="validationContext">The current validation context</param>
        /// <returns>A collection of validation errors</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Validate companies
            ValidateGroup(Companies, "companies", validationContext);
            // Validate people
            ValidateGroup(People, "people", validationContext);
            // Validate media
            ValidateGroup(Media, "media", validationContext);

            return errors;
        }

        /// <summary>
        /// Validates the specified group of relationships.
        /// </summary>
        /// <param name="group">The group of relationships to validate</param>
        /// <param name="groupName">The name of the group being validated</param>
        /// <param name="validationContext">The current validation context</param>
        /// <returns>A collection of validation errors</returns>
        private IEnumerable<ValidationResult> ValidateGroup(IEnumerable<RelationshipViewModel> group, string groupName, ValidationContext validationContext)
        {
            // Gaurd clause - The group is null
            if (group == null)
                return new List<ValidationResult>();

            List<ValidationResult> groupErrors = new List<ValidationResult>();
            int index = 0;
            foreach(RelationshipViewModel entry in group)
            {
                // If the entry has been altered and is invalid
                if(entry.HasBeenAltered && !entry.Validate())
                {
                    // Add an error
                    groupErrors.Add(new ValidationResult("Invalid relationship", new string[] { $"{groupName}:{index}" }));
                }

                // Update the index
                index++;
            }

            return groupErrors;
        }
    }

    // TODO: Relationship entries should probably be validated in the client directly and then confirmed here
    [JsonObject]
    public class RelationshipViewModel
    {
        // This property will be used when validating updated information
        // If true, then this relationship was not altered and should not be considered already validated.
        [JsonProperty("hasBeenAltered")]
        public bool HasBeenAltered { get; set; } = true;

        // This is the string identifier of the relationship type in the format sourceType-targetType
        [JsonProperty("relationshipType")]
        public string RelationshipType { get; set; }
        // This will always be a media node unless the relationship is not *-media (The target of the relationship)
        [JsonProperty("sourceId")]
        public Guid? OurId { get; set; }
        // This will always be a non-media node unless the relationship is media-* (The source of the relationship)
        [JsonProperty("otherId")]
        public Guid? OtherId { get; set; }
        [JsonProperty("otherName")]
        public string OtherName { get; set; }
        [JsonProperty("roles")]
        public IEnumerable<string> Roles { get; set; }

        /// <summary>
        /// Returns the roles as a single string.
        /// </summary>
        /// <returns>The roles of the relationship as a single string</returns>
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

        /// <summary>
        /// Determines if this relationship entry is valid.
        /// </summary>
        /// <returns>Returns true if the relationship entry is valid</returns>
        public bool Validate()
        {
            // Relationship type must be specified
            // Other Name must be specified
            // There must be at least one role
            return (!string.IsNullOrWhiteSpace(RelationshipType) && !string.IsNullOrWhiteSpace(OtherName) && Roles.Count() > 0);
        }
    }
}