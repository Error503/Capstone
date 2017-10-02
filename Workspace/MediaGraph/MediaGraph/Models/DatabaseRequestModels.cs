using MediaGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaGraph.Models
{
    public class DatabaseRequest
    {
        /**
         * Migration: "UpdateDatabaseModel"
         * Id converted to Guid from string
         * Foreign Key added to reviewer
         * Added ReviewedDate
         * 
         * Migration: "AddUpdateType"
         * Added Update type field to define what type of update the request defines
         * Added Foreign Key to an enum table of update types
         */

        // DatabaseRequest Data      
        [Key]
        public Guid Id { get; set; }
        //public RequestType UpdateType { get; set; }
        public string SubmitterRefId { get; set; }
        [ForeignKey("SubmitterRefId")]
        public ApplicationUser Submitter { get; set; }
        public DateTime SubmissionDate { get; set; }

        public string ReviewerRefId { get; set; }
        [ForeignKey("ReviewerRefId")]
        public ApplicationUser Reviewer { get; set; }
        public bool Reviewed { get; set; }
        public DateTime? ReviewedDate { get; set; } 
        public bool Approved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Notes { get; set; }

        // Node Data
        public string NodeData { get; set; }

        public NodeViewModel parseModel()
        {
            return JsonConvert.DeserializeObject<NodeViewModel>(NodeData) as NodeViewModel;
        }
    }

    public enum RequestType : int
    {
        Unknwon = 0,
        Add = 1,
        Update = 2,
        Delete = 3
    }

    [JsonObject]
    public class DatabaseRequestViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("requestType")]
        public RequestType RequestType { get; set; }
        [JsonProperty("submissionDate")]
        public DateTime SubmissionDate { get; set; }
        [JsonProperty("nodeData")]
        public NodeViewModel NodeData { get; set; }

        [JsonProperty("submitterId")]
        public string SubmitterId { get; set; }
        [JsonProperty("submitterName")]
        public string SubmitterName { get; set; }

        [JsonProperty("reviewerId")]
        public string ReviewerId { get; set; }
        [JsonProperty("reviewerName")]
        public string ReviewerName { get; set; }
        [JsonProperty("reviewDate")]
        public DateTime? ReviewDate { get; set; }
        [JsonProperty("reviewed")]
        public bool Reviewed { get; set; }
        [JsonProperty("approvalDate")]
        public DateTime? ApprovalDate { get; set; }
        [JsonProperty("approved")]
        public bool Approved { get; set; }
        [JsonProperty("notes")]
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
                RequestType = RequestType.Unknwon,
                SubmissionDate = model.SubmissionDate,
                NodeData = JsonConvert.DeserializeObject<NodeViewModel>(model.NodeData) as NodeViewModel,
                SubmitterId = model.SubmitterRefId,
                SubmitterName = submitter != null ? submitter.UserName : null,
                ReviewerId = model.ReviewerRefId,
                ReviewerName = reviewer != null ? reviewer.UserName : null,
                ReviewDate = model.ReviewedDate,
                Reviewed = model.Reviewed,
                ApprovalDate = model.ApprovalDate,
                Approved = model.Approved,
                Notes = model.Notes
            };
        }
    }
}