using MediaGraph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels
{
    public class FilterSettings
    {
        public string Submitter { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; } = DateTime.MinValue;
        public int RequestType { get; set; } = 0;

        public int HasBeenReviewed { get; set; } = -1;
        public int HasBeenApproved { get; set; } = -1;

        public int Page { get; set; } = 1;
        public int ResultsPerPage { get; set; } = 25;
    }

    [JsonObject]
    public class DatabaseRequestPage
    {
        [JsonProperty("requests")]
        public List<DatabaseRequestViewModel> Requests { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }
    }

    public class RequestReviewSubmission
    {
        public Guid RequestId { get; set; }
        public bool Approved { get; set; }
        public string Notes { get; set; }
    }
}