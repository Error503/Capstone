using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels.AdminTools
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class UserManagementFilter
    {
        [Display(Name = "User Name")]
        public string Email { get; set; }
        public string Role { get; set; }
        public int PageNumber { get; set; } = 1;
        [Display(Name = "Results Per Page")]
        public int ResultsPerPage { get; set; } = 25;
    }

    public class UserPage
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
    }
}