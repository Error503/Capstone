using MediaGraph.Models;
using MediaGraph.ViewModels.AdminTools;
using Microsoft.AspNet.Identity.EntityFramework;
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
            return View();
        }

        // POST: AdminTools/UserPage
        [HttpGet]
        public ActionResult UserPage(string userName, string userEmail, int page = 1, int resultsPerPage = 25)
        {
            ActionResult result = PartialView("_UserPagePartial", new List<ApplicationUser>());
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // TODO: User search functionality could be refined
                // Filter the users
                List<ApplicationUser> matchingUsers = (from user in context.Users
                                                      where (userName == null && userEmail == null) ||
                                                            (userName == null || user.UserName.Contains(userName)) ||
                                                            (userEmail == null || user.Email.Contains(userEmail))
                                                      orderby user.UserName
                                                      select user).ToList();
                // Get the result page
                int offset = (page - 1) * resultsPerPage;
                int pages = (int)Math.Ceiling(matchingUsers.Count / (double)resultsPerPage);
                // If there are enough results to make a page,
                if(matchingUsers.Count - offset >= resultsPerPage)
                {
                    // Get a full page
                    matchingUsers = matchingUsers.Skip(offset).Take(resultsPerPage).ToList();
                }
                else
                {
                    // Get the remaining users
                    matchingUsers = matchingUsers.Skip(offset).ToList();
                }

                List<UserViewModel> resultUsers = new List<UserViewModel>();
                // Convert the result to user view models
                for(int i = 0; i < matchingUsers.Count; i++)
                {
                    // Get the roles 
                    List<string> userRoles = new List<string>();
                    foreach (IdentityUserRole role in matchingUsers[i].Roles.OrderBy(role => role.RoleId))
                    {
                        userRoles.Add(context.Roles.Single(x => x.Id == role.RoleId).Name);
                    }

                    resultUsers.Add(new UserViewModel
                    {
                        Id = matchingUsers[i].Id,
                        Email = matchingUsers[i].Email,
                        Username = matchingUsers[i].UserName,
                        Role = userRoles[0]
                    });
                }

                result = PartialView("_UserPagePartial", resultUsers);
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
        public ActionResult UpdateUser(string userId, string role)
        {
            ActionResult result = Json(new { success = false, message = "Update failed" });
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Find the user
                ApplicationUser toUpdate = context.Users.SingleOrDefault(x => x.Id == userId);
                // If the user was found
                if(toUpdate != null)
                {
                    // Clear the user roles to ensure that no role is added twice
                    // This will also ensure than any roles removed will be removed
                    toUpdate.Roles.Clear();
                    IdentityUserRole userRole = new IdentityUserRole { RoleId = context.Roles.Single(x => x.Name == role).Id, UserId = userId };
                    // Add the user to the roles
                    toUpdate.Roles.Add(userRole);
                    context.SaveChanges();
                    result = Json(new { success = true, message = "User updated" });
                }
                else
                {
                    result = Json(new { success = false, message = "Failed to find the specified user" });
                }
            }

            return result;
        }
        #endregion

        #region Database Request Review
        // GET: AdminTools/DatabaseRequests
        public ActionResult DatabaseRequests()
        {
            return View();
        }

        // GET: AdminTools/RequestPage
        [HttpGet]
        public ActionResult RequestPage(int requestType, string commonName, int page = 1, int resultsPerPage = 25)
        {
            ActionResult result = Json(new { success = false, requests = new List<DatabaseRequest>() });
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                List<DatabaseRequest> requests = context.Requests.OrderBy(x => x.SubmissionDate).ToList();
                List<DatabaseRequest> resultRequests = null;
                // TODO: I could add search functionality to this
                int offset = (page - 1) * resultsPerPage;
                // If there are enough results to make a page
                if(requests.Count - offset >= resultsPerPage)
                {
                    resultRequests = requests.Skip(offset).Take(resultsPerPage).ToList();
                } 
                else
                {
                    resultRequests = requests.Skip(offset).ToList();
                }

                result = Json(new { totalPages = (int)Math.Ceiling(requests.Count / (double)resultsPerPage), requests = resultRequests }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        // GET: AdminTools/ViewRequest?id=
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
        #endregion
    }
}