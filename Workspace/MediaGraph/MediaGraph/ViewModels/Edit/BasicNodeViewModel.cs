using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MediaGraph.ViewModels.Edit 
{

    // TODO: Server-side validation of node and relationship view models

    [JsonObject]
    public class BasicNodeViewModel : IValidatableObject
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; } // Also "date of birth" or "date of founding"

        [JsonProperty("commonName")]
        [Display(Name = "Common English Name")]
        public string CommonName { get; set; }
        [JsonIgnore]
        public string OtherNames { get; set; }
        [JsonProperty("otherNames")]
        public IEnumerable<string> OtherNamesList { get; set; }

        // NOTE: When adding to the Neo4j database, use NodeType in this class to determine relationship direction
        [JsonIgnore]
        public string RelatedCompanies { get; set; }
        [JsonProperty("relatedCompanies")]
        public IEnumerable<RelationshipViewModel> RelatedCompaniesList { get; set; }
        [JsonIgnore]
        public string RelatedMedia { get; set; }
        [JsonProperty("relatedMedia")]
        public IEnumerable<RelationshipViewModel> RelatedMediaList { get; set; }
        [JsonIgnore]
        public string RelatedPeople { get; set; }
        [JsonProperty("relatedPeople")]
        public IEnumerable<RelationshipViewModel> RelatedPeopleList { get; set; }

        // Other possible properties
        //[JsonProperty("links")]
        //public Dictionary<WebsiteKey, string> AdditionalLinks { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Validate Id
            if (Id == Guid.Empty)
                errors.Add(new ValidationResult("An id has not been provided", new string[] { "Id" }));
            // Validate content type
            if (ContentType == null || ContentType == "")
                errors.Add(new ValidationResult("A content type must be provided", new string[] { "ContentType" }));
            // Validate CommonName
            if (string.IsNullOrWhiteSpace(CommonName))
                errors.Add(new ValidationResult("A common English name must be provided", new string[] { "CommonName" }));

            return errors;
        }

        #region JSON Conversion Methods
        /// <summary>
        /// Converts the date to the formatted string value.
        /// </summary>
        /// <param name="date">The date to convert</param>
        /// <returns>The formatted date value as a string</returns>
        public string GetDateValue(DateTime? date)
        {
            return date.HasValue ? date.Value.ToShortDateString() : "";
        }

        /// <summary>
        /// Converts the collection of other names into a JSON value to be put into forms.
        /// </summary>
        /// <returns>The JSON value of the other names collection</returns>
        public string GetOtherNamesJson()
        {
            return JsonConvert.SerializeObject(OtherNamesList);
        }

        /// <summary>
        /// Converts the collection of related companies into a JSON value to be 
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related companies collection</returns>
        public string GetRelatedCompaniesJson()
        {
            return JsonConvert.SerializeObject(RelatedCompaniesList);
        }

        /// <summary>
        /// Converts the collection of related media into a JSON value to be
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related media collection</returns>
        public string GetRelatedMediaJson()
        {
            return JsonConvert.SerializeObject(RelatedPeopleList);
        }

        /// <summary>
        /// Converts the collection of related people into a JSON value to be
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related people collection</returns>
        public string GetRelatedPeopleJson()
        {
            return JsonConvert.SerializeObject(RelatedPeopleList);
        }
        #endregion
    }

    [JsonObject]
    public class MediaNodeViewModel : BasicNodeViewModel
    {
        [JsonProperty("mediaType")]
        [Display(Name = "Media Type")]
        public string MediaType { get; set; }
        [JsonProperty("franchiseName")]
        [Display(Name = "Franchise")]
        public string FranchiseName { get; set; }
        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        // Other possible properties
        //public Dictionary<string, DateTime> RegionalReleaseDates { get; set; } // string key could be replaced with Region enum
        //public IEnumerable<string> Platforms { get; set; } // string could be replaced with a Platforms enum


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Create the list of errors by calling the base method
            List<ValidationResult> errors = new List<ValidationResult>(base.Validate(validationContext));

            // Validate MediaType
            if (string.IsNullOrWhiteSpace(MediaType))
                errors.Add(new ValidationResult("A media type must be provided", new string[] { "MediaType" }));

            return errors;
        }

        /// <summary>
        /// Converts the genres collection into a JSON object to be put into forms.
        /// </summary>
        /// <returns>The JSON value of the genres collection</returns>
        public string GetGenresJson()
        {
            return JsonConvert.SerializeObject(Genres);
        }
    }

    [JsonObject]
    public class PersonNodeViewModel : BasicNodeViewModel
    {
        // Other possible properties
        [JsonProperty("givenName")]
        [Display(Name = "Given Name")]
        public string GivenName { get; set; } 
        [JsonProperty("familyName")]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; }
        [JsonProperty("deathDate")]
        public DateTime? DeathDate { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

        // public string Nationality { get; set; } // string could be replaced with a Nationalities enum? - might be too large

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Validate GivenName
            if (string.IsNullOrWhiteSpace(GivenName))
                errors.Add(new ValidationResult("A given name must be provided", new string[] { "GivenName" }));
            // Validate FamilyName
            if (string.IsNullOrWhiteSpace(FamilyName))
                errors.Add(new ValidationResult("A family name must be provided", new string[] { "FamilyName" }));

            // Set the value of CommonName if not present
            if(string.IsNullOrWhiteSpace(CommonName))
                CommonName = $"{GivenName} {FamilyName}";

            // Validate that ReleaseDate comes before DeathDate
            if (ReleaseDate.HasValue && DeathDate.HasValue && ReleaseDate.Value.CompareTo(DeathDate.Value) > 0)
                errors.Add(new ValidationResult("Date of birth must come before date of death", new string[] { "ReleaseDate", "DeathDate" }));
            // Correct invalid status if the person has passed away
            if (DeathDate.HasValue && Status != "deceased")
                Status = "deceased";

            // Call the base method
            errors.AddRange(base.Validate(validationContext));

            return errors;
        }
    }

    [JsonObject]
    public class CompanyNodeViewModel : BasicNodeViewModel
    {
        // Other possible properties
        [JsonProperty("deathDate")]
        public DateTime? DeathDate { get; set; } // Date that the company ended

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>(base.Validate(validationContext));

            // Validate that release date comes before death date
            if (ReleaseDate.HasValue && DeathDate.HasValue && ReleaseDate.Value.CompareTo(DeathDate.Value) > 0)
                errors.Add(new ValidationResult("Date founded must come before the date of death", new string[] { "ReleaseDate", "DeathDate" }));

            return errors;
        }
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