using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel.DataAnnotations;
using MediaGraph.Models.Component;
using System.Web.Http.ModelBinding;
using MediaGraph.Code;

namespace MediaGraph.ViewModels.Edit 
{

    [JsonObject]
    public class BasicNodeViewModel : IValidatableObject
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("contentType")]
        public NodeContentType ContentType { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; } // Also "date of birth" or "date of founding"
        [JsonProperty("deathDate")]
        public DateTime? DeathDate { get; set; }

        [JsonProperty("commonName")]
        [Display(Name = "Common English Name")]
        public string CommonName { get; set; }
        [JsonProperty("otherNames")]
        public IEnumerable<string> OtherNames { get; set; } = new List<string>();

        [JsonProperty("relatedCompanies")]
        public IEnumerable<RelationshipViewModel> RelatedCompanies { get; set; } = new List<RelationshipViewModel>();
        [JsonProperty("relatedMedia")]
        public IEnumerable<RelationshipViewModel> RelatedMedia { get; set; } = new List<RelationshipViewModel>();
        [JsonProperty("relatedPeople")]
        public IEnumerable<RelationshipViewModel> RelatedPeople { get; set; } = new List<RelationshipViewModel>();

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
            if (ContentType == 0)
                errors.Add(new ValidationResult("A content type must be provided", new string[] { "ContentType" }));
            // Validate CommonName
            if (string.IsNullOrWhiteSpace(CommonName))
                errors.Add(new ValidationResult("A common English name must be provided", new string[] { "CommonName" }));
            // Validate that release date comes before death date
            if (ReleaseDate.HasValue && DeathDate.HasValue && ReleaseDate.Value.CompareTo(DeathDate.Value) > 0)
                errors.Add(new ValidationResult("Date founded must come before the date of death", new string[] { "ReleaseDate", "DeathDate" }));

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
            return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "";
        }

        /// <summary>
        /// Converts the collection of other names into a JSON value to be put into forms.
        /// </summary>
        /// <returns>The JSON value of the other names collection</returns>
        public string GetOtherNamesJson()
        {
            return JsonConvert.SerializeObject(OtherNames);
        }

        /// <summary>
        /// Converts the collection of related companies into a JSON value to be 
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related companies collection</returns>
        public string GetRelatedCompaniesJson()
        {
            return JsonConvert.SerializeObject(RelatedCompanies);
        }

        /// <summary>
        /// Converts the collection of related media into a JSON value to be
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related media collection</returns>
        public string GetRelatedMediaJson()
        {
            return JsonConvert.SerializeObject(RelatedMedia);
        }

        /// <summary>
        /// Converts the collection of related people into a JSON value to be
        /// put into forms.
        /// </summary>
        /// <returns>The JSON value of the related people collection</returns>
        public string GetRelatedPeopleJson()
        {
            return JsonConvert.SerializeObject(RelatedPeople);
        }

        /// <summary>
        /// Serializes this node to a JSON string based on the content type of the node.
        /// </summary>
        /// <returns>The JSON string representing this node</returns>
        public string SerializeToContentType()
        {
            string value = null;
            if (ContentType == NodeContentType.Company)
                value = JsonConvert.SerializeObject((CompanyNodeViewModel)this);
            else if (ContentType == NodeContentType.Media)
                value = JsonConvert.SerializeObject((MediaNodeViewModel)this);
            else if (ContentType == NodeContentType.Person)
                value = JsonConvert.SerializeObject((PersonNodeViewModel)this);
            return value;
        }
        #endregion

        /// <summary>
        /// Constructs a node view model from the given node model.
        /// </summary>
        /// <param name="model">The node model for which to create a view model</param>
        /// <returns>The created view model</returns>
        public static BasicNodeViewModel FromModel(BasicNodeModel model)
        {
            BasicNodeViewModel result = null;
            if(model.ContentType == NodeContentType.Company)
            {
                result = new CompanyNodeViewModel();
            }
            else if(model.ContentType == NodeContentType.Media)
            {
                result = new MediaNodeViewModel
                {
                    MediaType = ((MediaNodeModel)model).MediaType,
                    FranchiseName = ((MediaNodeModel)model).FranchiseName,
                    Genres = ((MediaNodeModel)model).Genres,
                };
            } 
            else if(model.ContentType == NodeContentType.Person)
            {
                result = new PersonNodeViewModel
                {
                    GivenName = ((PersonNodeModel)model).GivenName,
                    FamilyName = ((PersonNodeModel)model).FamilyName,
                    Status = ((PersonNodeModel)model).Status
                };
            }
            // Add basic information
            result.Id = model.Id;
            result.ContentType = model.ContentType;
            result.CommonName = model.CommonName;
            result.OtherNames = model.OtherNames;
            result.ReleaseDate = model.ReleaseDate.HasValue ? DateValueConverter.ToDateTime(model.ReleaseDate.Value) : default(DateTime?);
            result.DeathDate = model.DeathDate.HasValue ? DateValueConverter.ToDateTime(model.DeathDate.Value) : default(DateTime?);
            // Add the relationships
            result.RelatedCompanies = ConvertRelationships(model.RelatedCompanies);
            result.RelatedMedia = ConvertRelationships(model.RelatedMedia);
            result.RelatedPeople = ConvertRelationships(model.RelatedPeople);

            return result;
        }


        /// <summary>
        /// Converts a collection of relationship models into a collection of relationship view models.
        /// </summary>
        /// <param name="relationships">The collection of relationship models</param>
        /// <returns>A collection of relationship view models</returns>
        private static List<RelationshipViewModel> ConvertRelationships(IEnumerable<RelationshipModel> relationships)
        {
            List<RelationshipViewModel> result = new List<RelationshipViewModel>();

            foreach(RelationshipModel model in relationships)
            {
                result.Add(RelationshipViewModel.FromModel(model));
            }

            return result;
        }

        #region ToModel Method
        public virtual BasicNodeModel ToModel()
        {
            return new BasicNodeModel
            {
                Id = this.Id,
                ContentType = this.ContentType,
                CommonName = this.CommonName,
                OtherNames = this.OtherNames,
                ReleaseDate = this.ReleaseDate.HasValue ? DateValueConverter.ToLongValue(this.ReleaseDate.Value) : default(long?),
                DeathDate = this.DeathDate.HasValue ? DateValueConverter.ToLongValue(this.DeathDate.Value) : default(long?),
                RelatedCompanies = ConvertRelationshipsToModel(this.RelatedCompanies),
                RelatedMedia = ConvertRelationshipsToModel(this.RelatedMedia),
                RelatedPeople = ConvertRelationshipsToModel(this.RelatedPeople)
            };
        }

        protected List<RelationshipModel> ConvertRelationshipsToModel(IEnumerable<RelationshipViewModel> viewModel)
        {
            List<RelationshipModel> relationships = new List<RelationshipModel>();

            foreach(RelationshipViewModel vm in viewModel)
            {
                relationships.Add(new RelationshipModel
                {
                    SourceId = vm.SourceId,
                    TargetId = vm.TargetId,
                    TargetName = vm.TargetName,
                    Roles = vm.Roles
                });
            }

            return relationships;
        }
        #endregion
    }

    public class MediaNodeViewModel : BasicNodeViewModel
    {
        [JsonProperty("mediaType")]
        [Display(Name = "Media Type")]
        public NodeMediaType MediaType { get; set; }
        [JsonProperty("franchise")]
        [Display(Name = "Franchise")]
        public string FranchiseName { get; set; }
        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        // Other possible properties
        //public Dictionary<string, DateTime> RegionalReleaseDates { get; set; } // string key could be replaced with Region enum
        //public IEnumerable<string> Platforms { get; set; } // string could be replaced with a Platforms enum

        public MediaNodeViewModel() : base() { ContentType = NodeContentType.Media; }


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Create the list of errors by calling the base method
            List<ValidationResult> errors = new List<ValidationResult>(base.Validate(validationContext));

            // Validate MediaType
            if (MediaType == 0)
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

        public override BasicNodeModel ToModel()
        {
            return new MediaNodeModel
            {
                Id = this.Id,
                ContentType = this.ContentType,
                CommonName = this.CommonName,
                OtherNames = this.OtherNames,
                ReleaseDate = this.ReleaseDate.HasValue ? DateValueConverter.ToLongValue(this.ReleaseDate.Value) : default(long?),
                DeathDate = this.DeathDate.HasValue ? DateValueConverter.ToLongValue(this.DeathDate.Value) : default(long?),
                MediaType = this.MediaType,
                FranchiseName = this.FranchiseName,
                Genres = this.Genres,
                RelatedCompanies = ConvertRelationshipsToModel(this.RelatedCompanies),
                RelatedMedia = ConvertRelationshipsToModel(this.RelatedMedia),
                RelatedPeople = ConvertRelationshipsToModel(this.RelatedPeople)
            };
        }
    }

    public class PersonNodeViewModel : BasicNodeViewModel
    {
        // Other possible properties
        [JsonProperty("givenName")]
        [Display(Name = "Given Name")]
        public string GivenName { get; set; } 
        [JsonProperty("familyName")]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; }
        [JsonProperty("status")]
        public PersonStatus Status { get; set; }

        // public string Nationality { get; set; } // string could be replaced with a Nationalities enum? - might be too large

        public PersonNodeViewModel() : base() { ContentType = NodeContentType.Person; }

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

            // Correct invalid status if the person has passed away
            if (DeathDate.HasValue && Status != PersonStatus.Deceased)
                Status = PersonStatus.Deceased;

            // Call the base method
            errors.AddRange(base.Validate(validationContext));

            return errors;
        }

        public override BasicNodeModel ToModel()
        {
            return new PersonNodeModel
            {
                Id = this.Id,
                ContentType = this.ContentType,
                CommonName = this.CommonName,
                OtherNames = this.OtherNames,
                ReleaseDate = this.ReleaseDate.HasValue ? DateValueConverter.ToLongValue(this.ReleaseDate.Value) : default(long?),
                DeathDate = this.DeathDate.HasValue ? DateValueConverter.ToLongValue(this.DeathDate.Value) : default(long?),
                FamilyName = this.FamilyName,
                GivenName = this.FamilyName,
                Status = this.Status,
                RelatedCompanies = ConvertRelationshipsToModel(this.RelatedCompanies),
                RelatedMedia = ConvertRelationshipsToModel(this.RelatedMedia),
                RelatedPeople = ConvertRelationshipsToModel(this.RelatedPeople)
            };
        }
    }

    public class CompanyNodeViewModel : BasicNodeViewModel
    {
        // CompanyNodeViewModel adds nothing to the basic view model
        public CompanyNodeViewModel() : base() { ContentType = NodeContentType.Company; }
    }

    public class RelationshipViewModel
    {
        public Guid SourceId { get; set; }
        public Guid? TargetId { get; set; }
        public string TargetName { get; set; }
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

        /// <summary>
        /// Creates a relationship view model from the given relationship model.
        /// </summary>
        /// <param name="model">The relationship model</param>
        /// <returns>The created relationship view model</returns>
        public static RelationshipViewModel FromModel(RelationshipModel model)
        {
            return new RelationshipViewModel
            {
                SourceId = model.SourceId,
                TargetId = model.TargetId,
                TargetName = model.TargetName,
                Roles = model.Roles
            };
        }
    }
}