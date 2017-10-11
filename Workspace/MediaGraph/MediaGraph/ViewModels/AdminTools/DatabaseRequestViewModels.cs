using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MediaGraph.Models;

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
}