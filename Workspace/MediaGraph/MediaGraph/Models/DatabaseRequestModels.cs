using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

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
         * Migration: "RequestAndNodeType" TO BE ADDED
         * Added RequestType 
         * Added NodeDataType 
         */

        // DatabaseRequest Data      
        [Key]
        public Guid Id { get; set; }
        public DatabaseRequestType RequestType  { get; set; }
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

        // Node data
        public NodeContentType NodeDataType { get; set; }
        public string NodeData { get; set; }

        /// <summary>
        /// Parses the node data string
        /// </summary>
        /// <returns>The </returns>
        public BasicNodeViewModel ParseModel()
        {
            BasicNodeViewModel viewModel = null;
            if(NodeDataType == NodeContentType.Company)
            {
                viewModel = JsonConvert.DeserializeObject<CompanyNodeViewModel>(NodeData) as CompanyNodeViewModel;
            }
            else if(NodeDataType == NodeContentType.Media)
            {
                viewModel = JsonConvert.DeserializeObject<MediaNodeViewModel>(NodeData) as MediaNodeViewModel;
            }
            else if(NodeDataType == NodeContentType.Person)
            {
                viewModel = JsonConvert.DeserializeObject<PersonNodeViewModel>(NodeData) as PersonNodeViewModel;
            }

            return viewModel;
        }
    }

    public enum DatabaseRequestType : byte
    {
        Unknown = 0,
        Add = 1,
        Update = 2,
        Delete = 3
    }

    public class DatabaseRequestViewModel
    {
        public Guid Id { get; set; }
        public DatabaseRequestType RequestType { get; set; }
        public DateTime SubmissionDate { get; set; }
        public BasicNodeViewModel NodeData { get; set; }

        public string SubmitterId { get; set; }
        public string SubmitterName { get; set; }

        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool Reviewed { get; set; }
        public DateTime? ApprovalDate { get; set; }
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
                RequestType = DatabaseRequestType.Unknown,
                SubmissionDate = model.SubmissionDate,
                NodeData = model.ParseModel(),
                //NodeDataType = model.NodeDataType,
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