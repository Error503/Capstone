using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.AdminTools;
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
    public class NodeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("Binding context is null", "bindingContext");
            }
            HttpRequestBase request = controllerContext.HttpContext.Request;
            BasicNodeViewModel result = null;

            NodeContentType contentType = (NodeContentType)Enum.Parse(typeof(NodeContentType), request.Form["ContentType"]);
            DateTime? releaseDate = ParseDateTime(bindingContext.ValueProvider.GetValue("ReleaseDate").AttemptedValue);
            DateTime? deathDate = contentType == NodeContentType.Media ? null : ParseDateTime(bindingContext.ValueProvider.GetValue("DeathDate").AttemptedValue);

            if (contentType == NodeContentType.Company)
            {
                result = new CompanyNodeViewModel();
            }
            else if (contentType == NodeContentType.Media)
            {
                result = new MediaNodeViewModel
                {
                    Genres = ParseFromJson<IEnumerable<string>>(request.Form["Genres"]),
                    MediaType = (NodeMediaType)Enum.Parse(typeof(NodeMediaType), request.Form["MediaType"]),
                    FranchiseName = request.Form["FranchiseName"].Trim()
                };
            }
            else if (contentType == NodeContentType.Person)
            {
                result = new PersonNodeViewModel
                {
                    GivenName = request.Form["GivenName"].Trim(),
                    FamilyName = request.Form["FamilyName"].Trim(),
                    Status = (PersonStatus)Enum.Parse(typeof(PersonStatus), request.Form["Status"])
                };
            }

            result.Id = Guid.Parse(request.Form["Id"]);
            result.ContentType = contentType;
            result.CommonName = contentType == NodeContentType.Person ? $"{((PersonNodeViewModel)result).GivenName} {((PersonNodeViewModel)result).FamilyName}" : request.Form["CommonName"].Trim();
            result.OtherNames = ParseFromJson<IEnumerable<string>>(request.Form["OtherNames"]);
            result.ReleaseDate = releaseDate;
            result.DeathDate = deathDate;
            result.Relationships = ParseFromJson<IEnumerable<RelationshipViewModel>>(request.Form["Relationships"]);

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

    public class RequestReviewModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if(bindingContext == null)
            {
                throw new ArgumentNullException("Binding context is null", "bindingContext");
            }

            RequestReviewViewModel result = new RequestReviewViewModel();
            Guid requestId;
            DatabaseRequestType requestType;
            NodeContentType nodeDataType;
            bool approved;
            BasicNodeViewModel nodeViewModel = (new NodeModelBinder()).BindModel(controllerContext, bindingContext) as BasicNodeViewModel;

            if(!Guid.TryParse(GetAttemptedValue(bindingContext, "RequestId"), out requestId)) {
                bindingContext.ModelState.AddModelError("RequestId", "Invalid request id");
            }
            if(!Enum.TryParse<DatabaseRequestType>(GetAttemptedValue(bindingContext, "RequestType"), out requestType))
            {
                bindingContext.ModelState.AddModelError("RequestType", "Invalid request type");
            }
            if(!Enum.TryParse<NodeContentType>(GetAttemptedValue(bindingContext, "NodeDataType"), out nodeDataType))
            {
                bindingContext.ModelState.AddModelError("NodeDataType", "Invalid node data type");
            }
            if(!bool.TryParse(GetAttemptedValue(bindingContext, "Approved"), out approved))
            {
                bindingContext.ModelState.AddModelError("Approved", "Invalid approval state");
            }

            if(bindingContext.ModelState.IsValid)
            {
                result = new RequestReviewViewModel
                {
                    RequestId = requestId,
                    RequestType = requestType,
                    NodeDataType = nodeDataType,
                    NodeData = nodeViewModel,
                    Notes = GetAttemptedValue(bindingContext, "Notes"),
                    Approved = approved
                };
            }

            return result;
        }

        private string GetAttemptedValue(ModelBindingContext context, string key)
        {
            return context.ValueProvider.GetValue(key).AttemptedValue;
        }
    }
}