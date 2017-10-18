using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.AdminTools;
using MediaGraph.ViewModels.Edit;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MediaGraph.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class AdminToolsController : Controller
    {
        // Exclude the default admin, myself, from being displayed
        private static readonly string[] kExcludedUsers = new string[] { "2f1c3a70-493a-42b3-a29e-c3ec96acc4a6" };

        // GET: AdminTools
        public ActionResult Index()
        {
            return View();
        }

        #region User Management
        // GET: AdminTools/UserManagement
        [Authorize(Roles = "staff")]
        public ActionResult UserManagement()
        {
            return View(model: GetUserPage(new UserManagementFilter()));
        }

        [HttpPost]
        public ActionResult UserPage(UserManagementFilter filter)
        {
            return Json(GetUserPage(filter));
        }

        private UserPage GetUserPage(UserManagementFilter filter)
        {
            UserPage result = new ViewModels.AdminTools.UserPage { CurrentPage = 0, TotalPages = 0, Users = new List<UserViewModel>() };
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Filter the users by user email (user name)
                List<ApplicationUser> matchingUsers = (from user in context.Users
                                                       where ((filter.Email == null || filter.Email == "") || user.Email.Contains(filter.Email)) &&
                                                       (!kExcludedUsers.Contains(user.Id))
                                                       select user).ToList();

                List<UserViewModel> resultUsers = new List<UserViewModel>();

                // Filter the users by role
                foreach (ApplicationUser user in matchingUsers)
                {
                    string role = user.Roles.Single().RoleId;
                    if (string.IsNullOrEmpty(filter.Role) || role == filter.Role)
                    {
                        resultUsers.Add(new UserViewModel { Id = user.Id, Email = user.Email, Username = user.UserName, Role = role });
                    }
                }

                // Get the result page
                int offset = (filter.PageNumber - 1) * filter.ResultsPerPage;
                int pages = (int)Math.Ceiling(resultUsers.Count / (double)filter.ResultsPerPage);
                // If there are enough results to make a page,
                if (matchingUsers.Count - offset >= filter.ResultsPerPage)
                {
                    // Get a full page
                    resultUsers = resultUsers.Skip(offset).Take(filter.ResultsPerPage).ToList();
                }
                else
                {
                    // Get the remaining users
                    resultUsers = resultUsers.Skip(offset).ToList();
                }

                result = new UserPage { CurrentPage = filter.PageNumber, TotalPages = pages, Users = resultUsers };
            }

            return result;
        }

        [HttpGet]
        public ActionResult GetUserPage(string username, string email, int page = 1, int resultsPerPage = 25)
        {
            ActionResult result = Json(new { PageCount = 0, Users = 0 }, JsonRequestBehavior.AllowGet);
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Filter the user list
                List<ApplicationUser> matchingUsers = (from user in context.Users
                                                       where !kExcludedUsers.Contains(user.Id) &&
                                                             (username == string.Empty && email == string.Empty) ||
                                                             (username != string.Empty && user.UserName.Contains(username)) ||
                                                             (email != string.Empty && user.Email.Contains(email))
                                                       orderby user.UserName
                                                       select user).ToList();
                // Get the total number of pages
                int totalPages = (int)Math.Ceiling(matchingUsers.Count / (double)resultsPerPage);
                // Calculate the offset
                int offset = (page - 1) * resultsPerPage;
                // Get the user page
                if (matchingUsers.Count - offset > resultsPerPage)
                {
                    // Get a full page of users
                    matchingUsers = matchingUsers.Skip(offset).Take(resultsPerPage).ToList();
                }
                else
                {
                    // Get the remaining users
                    matchingUsers = matchingUsers.Skip(offset).ToList();
                }

                // Convert the results 
                List<UserViewModel> resultUsers = new List<UserViewModel>();
                for (int i = 0; i < matchingUsers.Count; i++)
                {
                    ApplicationUser user = matchingUsers[i];
                    resultUsers.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        Role = user.Roles.First().RoleId
                    });
                }

                result = Json(new { PageCount = totalPages, Users = resultUsers }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // AdminTools/UpdateUser
        [HttpPost]
        public ActionResult UpdateUser(string id, string role)
        {
            ActionResult result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Find the user
                ApplicationUser toUpdate = context.Users.SingleOrDefault(x => x.Id == id);
                // If the user was found
                if (toUpdate != null)
                {
                    // Make sure that the role is 
                    if (toUpdate.Roles.Single().RoleId != role)
                    {
                        // Clear the user roles to ensure that no role is added twice
                        // This will also ensure than any roles removed will be removed
                        toUpdate.Roles.Clear();
                        IdentityUserRole userRole = new IdentityUserRole { RoleId = context.Roles.Single(x => x.Id == role).Id, UserId = id };
                        // Add the user to the roles
                        toUpdate.Roles.Add(userRole);
                        context.SaveChanges();
                        result = new HttpStatusCodeResult(HttpStatusCode.Accepted);
                    }
                    else
                    {
                        result = new HttpStatusCodeResult(HttpStatusCode.NotModified);
                    }
                }
                else
                {
                    result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }

            return result;
        }
        #endregion

        #region Database Request Review
        // GET: AdminTools/DatabaseRequests
        public ActionResult DatabaseRequests()
        {
            return View(model: GetRequestPage(new DatabaseRequestFilter()));
        }

        public ActionResult RequestPage(DatabaseRequestFilter filter)
        {
            return Json(GetRequestPage(filter));
        }

        private DatabaseRequestPage GetRequestPage(DatabaseRequestFilter filter)
        {
            DatabaseRequestPage result = new DatabaseRequestPage();
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                List<DatabaseRequest> requests = (from req in context.Requests
                                                  where (filter.RequestType == 0 || filter.RequestType == req.RequestType) &&
                                                  ((filter.SubmittedBy == null || filter.SubmittedBy.Length == 0) || req.Submitter.UserName.Contains(filter.SubmittedBy)) &&
                                                  (!filter.LowerBoundDate.HasValue || filter.LowerBoundDate.Value.CompareTo(req.SubmissionDate) <= 0) &&
                                                  (!filter.UpperBoundDate.HasValue || filter.UpperBoundDate.Value.CompareTo(req.SubmissionDate) >= 0)
                                                  orderby req.SubmissionDate
                                                  select req
                                                 ).ToList();
                List<DatabaseRequestViewModel> resultRequests = null;
                int offset = (filter.PageNumber - 1) * filter.ResultsPerPage;
                // If there are enough results to make a page
                if (requests.Count - offset >= filter.ResultsPerPage)
                {
                    resultRequests = requests.Select(y => DatabaseRequestViewModel.FromModel(y)).Where(x => ((filter.NodeName == null || filter.NodeName == "") || x.NodeData.CommonName.Contains(filter.NodeName)))
                        .Skip(offset).Take(filter.ResultsPerPage).ToList();
                }
                else
                {
                    resultRequests = requests.Select(y => DatabaseRequestViewModel.FromModel(y)).Where(x => ((filter.NodeName == null || filter.NodeName == "") || x.NodeData.CommonName.Contains(filter.NodeName)))
                        .Skip(offset).ToList();
                }

                result = new DatabaseRequestPage
                {
                    TotalPages = (int)Math.Ceiling(requests.Count / (double)filter.ResultsPerPage),
                    CurrentPage = filter.PageNumber,
                    Requests = resultRequests
                };
            }

            return result;
        }

        // GET: AdminTools/ViewRequest/id
        [HttpGet]
        public ActionResult ViewRequest(Guid id)
        {
            ActionResult result = View("Error");

            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                DatabaseRequest request = context.Requests.SingleOrDefault(x => x.Id == id);
                if(request != null)
                {
                    result = View(model: DatabaseRequestViewModel.FromModel(request));
                }
            }

            return result;
        }

        [HttpPost]
        public ActionResult ViewRequest(RequestReviewViewModel request)
        {
            // TODO: Editing that results in an invalid node
            ActionResult result = View("Error");
            if (request != null)
            {
                DatabaseRequest fromDatabase;
                bool shouldCommitChanges = false;
                // Get the data from the request database
                using (ApplicationDbContext context = ApplicationDbContext.Create())
                {
                    fromDatabase = context.Requests.Single(x => x.Id == request.NodeData.Id);
                    shouldCommitChanges = !fromDatabase.Reviewed && request.Approved;
                    // Update the information in the database
                    fromDatabase.Reviewed = true;
                    fromDatabase.ReviewedDate = DateTime.Now;
                    fromDatabase.Approved = request.Approved;
                    if (request.Approved)
                        fromDatabase.ApprovalDate = DateTime.Now;
                    // Save the changes
                    context.SaveChanges();
                }

                // If the request has not been reviewed,
                if (shouldCommitChanges)
                {
                    // Commit the changes to the database
                    CheckRequestsAndCommitChanges(request.NodeData, fromDatabase);
                }

                result = RedirectToAction("DatabaseRequests");
            }

            return result;
        }

        private void CheckRequestsAndCommitChanges(BasicNodeViewModel fromForm, DatabaseRequest fromDatabase)
        {
            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                if(fromDatabase.RequestType == DatabaseRequestType.Add)
                {
                    driver.AddNode(fromForm.ToModel());
                }
                else if(fromDatabase.RequestType == DatabaseRequestType.Update)
                {
                    driver.UpdateNode(fromForm.ToModel());
                }
                else if(fromDatabase.RequestType == DatabaseRequestType.Delete)
                {
                    driver.DeleteNode(fromForm.Id);
                }
            }
        }
        #endregion
    }
}