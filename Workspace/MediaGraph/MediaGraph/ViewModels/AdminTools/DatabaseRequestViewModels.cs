using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels.AdminTools
{
    public class DatabaseRequestFilter
    {
        [Display(Name = "Results Per Page")]
        public int ResultsPerPage { get; set; } = 25;
        public int PageNumber { get; set; } = 1;
        [Display(Name = "Submitted by")]
        public string SubmittedBy { get; set; } = null;
        [Display(Name = "Node Name")]
        public string NodeName { get; set; } = null;
        [Display(Name = "Request Type")]
        public DatabaseRequestType RequestType { get; set; }
        [Display(Name = "Submitted After")]
        public DateTime? LowerBoundDate { get; set; }
        [Display(Name = "Submitted Before")]
        public DateTime? UpperBoundDate { get; set; }
    }

    public class DatabaseRequestPage
    {
        public int TotalPages { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
        public IEnumerable<DatabaseRequestViewModel> Requests { get; set; } = new List<DatabaseRequestViewModel>();
    }


    public class DatabaseRequestViewModel
    {
        public Guid Id { get; set; }
        public DatabaseRequestType RequestType { get; set; }
        public NodeContentType NodeDataType { get; set; }
        public BasicNodeViewModel NodeData { get; set; }

        public string SubmitterId { get; set; }
        public string SubmitterName { get; set; }
        public DateTime SubmissionDate { get; set; }

        public bool Reviewed { get; set; } 
        public bool Approved { get; set; }
        public string Notes { get; set; }

        /// <summary>
        /// Static method that creates a DatabaseRequestViewModel from the 
        /// given DatabaseRequest object.
        /// </summary>
        /// <param name="model">The DatabaseRequest object from which to create
        /// a view model</param>
        /// <returns>The view model for the given database request object</returns>
        public static DatabaseRequestViewModel FromModel(DatabaseRequest model)
        {
            ApplicationUser submitter;
            ApplicationUser reviewer;
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                submitter = context.Users.SingleOrDefault(u => u.Id == model.SubmitterRefId);
                reviewer = context.Users.SingleOrDefault(u => u.Id == model.ReviewerRefId);
            }
            return new DatabaseRequestViewModel
            {
                Id = model.Id,
                RequestType = model.RequestType,
                SubmissionDate = model.SubmissionDate,
                NodeData = model.ParseModel(),
                NodeDataType = model.NodeDataType,
                SubmitterId = model.SubmitterRefId,
                SubmitterName = submitter != null ? submitter.UserName : null,
                Reviewed = model.Reviewed,
                Approved = model.Approved,
                Notes = model.Notes
            };
        }

        /// <summary>
        /// Returns the string representation of the request type.
        /// </summary>
        /// <returns>The string representation of this request's type</returns>
        public string GetRequestTypeString()
        {
            return RequestType == 0 ? "Unknown" : Enum.GetName(typeof(DatabaseRequestType), RequestType);
        }
    }

    public class RequestReviewViewModel
    {
        public Guid RequestId { get; set; }
        public DatabaseRequestType RequestType { get; set; }
        public NodeContentType NodeDataType { get; set; }
        public BasicNodeViewModel NodeData { get; set; }

        public string Notes { get; set; }
        public bool Approved { get; set; }
    }
}