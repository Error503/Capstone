using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace MediaGraph.Code
{
    public class CustomDateBinder : IModelBinder
    {

        public object BindModel(ControllerContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            DateTime? dateTimeValue = null;
            DateTime parsed;
            // If the value was retreived
            if (!DateTime.TryParse(bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue, out parsed))
            {
                // Attempt a manual parsing
                string rawInput = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
                string[] split = rawInput.Split('-');
                int year = 0, month = 0, day = 0;

                if (int.TryParse(split[0], out year) && int.TryParse(split[1], out month) && int.TryParse(split[2], out day))
                {
                    dateTimeValue = new DateTime(year, month, day);
                }
            } 
            else
            {
                dateTimeValue = parsed;
            }

            return dateTimeValue;
        }
    }

    public class RelationshipCollectionBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Attempt to parse the value as a collection of relationship view models
            IEnumerable<RelationshipViewModel> result = JsonConvert.DeserializeObject<List<RelationshipViewModel>>(
                bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue) as List<RelationshipViewModel>;

            // Return an empty collection if parsing failed
            return result ?? new List<RelationshipViewModel>();
        }
    }

    public class StringCollectionBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            // Attempt to parse the value as a collection of string
            IEnumerable<string> result = JsonConvert.DeserializeObject<IEnumerable<string>>(
                bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue) as IEnumerable<string>;

            // Return an empty collection if parsing failed
            return result ?? new List<string>();
        }
    }

    public class NodeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            BasicNodeViewModel result = null;

            NodeContentType contentType = (NodeContentType)Enum.Parse(typeof(NodeContentType), request.Form["ContentType"]);
            DateTime? releaseDate = ParseDateTime(bindingContext.ValueProvider.GetValue("ReleaseDate").AttemptedValue);
            DateTime? deathDate = ParseDateTime(bindingContext.ValueProvider.GetValue("DeathDate").AttemptedValue);

            if(contentType == NodeContentType.Company)
            {
                result = new CompanyNodeViewModel();
            }
            else if(contentType == NodeContentType.Media)
            {
                result = new MediaNodeViewModel
                {
                    Genres = ParseFromJson<IEnumerable<string>>(request.Form["Genres"]),
                    MediaType = (NodeMediaType)Enum.Parse(typeof(NodeMediaType), request.Form["MediaType"]),
                    FranchiseName = request.Form["FranchiseName"]
                };
            }
            else if(contentType == NodeContentType.Person)
            {
                result = new PersonNodeViewModel
                {
                    GivenName = request.Form["GivenName"],
                    FamilyName = request.Form["FamilyName"],
                    Status = (PersonStatus)Enum.Parse(typeof(PersonStatus), request.Form["Status"])
                };
            }

            result.Id = Guid.Parse(request.Form["Id"]);
            result.ContentType = contentType;
            result.CommonName = request.Form["CommonName"];
            result.OtherNames = ParseFromJson<IEnumerable<string>>(request.Form["OtherNames"]);
            result.ReleaseDate = releaseDate;
            result.DeathDate = deathDate;
            result.RelatedCompanies = ParseFromJson<IEnumerable<RelationshipViewModel>>(request.Form["RelatedCompanies"]);
            result.RelatedMedia = ParseFromJson<IEnumerable<RelationshipViewModel>>(request.Form["RelatedMedia"]);
            result.RelatedPeople = ParseFromJson<IEnumerable<RelationshipViewModel>>(request.Form["RelatedPeople"]);

            // Validate the object
            Validate(result, bindingContext);

            return result;
        }

        /// <summary>
        /// Run the validator to validate the object and add validation errors to the model state.
        /// </summary>
        /// <param name="boundObject">The object being bound</param>
        /// <param name="context">The model binding context</param>
        private void Validate(BasicNodeViewModel boundObject, ModelBindingContext context)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            // If the model is not valid
            if (!Validator.TryValidateObject(boundObject, new ValidationContext(boundObject, null, null), errors, true))
            {
                // Add the model errors to the ModelState
                foreach(ValidationResult err in errors)
                {
                    foreach(string propertyKey in err.MemberNames)
                    {
                        context.ModelState.AddModelError(propertyKey, err.ErrorMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to parse a nullable DateTime from the given string.
        /// </summary>
        /// <param name="dateTime">The string of version of the DateTime to parse</param>
        /// <returns>The nullable DateTime resulting from the parsing</returns>
        private DateTime? ParseDateTime(string dateTime)
        {
            DateTime? result = null;
            DateTime temp;
            // Try to parse the date directly,
            if(DateTime.TryParse(dateTime, out temp))
            {
                result = temp;
            }
            else
            {
                // Try to parse the date manually
                int year;
                int month;
                int day;
                string[] parts = dateTime.Split('-');
                if (int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month) && int.TryParse(parts[2], out day))
                {
                    result = new DateTime(year, month, day);
                }
            }

            return result;
        }

        private T ParseFromJson<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}