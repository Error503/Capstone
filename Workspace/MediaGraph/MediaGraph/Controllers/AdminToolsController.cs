﻿using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.ViewModels.AdminTools;
using MediaGraph.ViewModels.Edit;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class AdminToolsController : Controller
    {
        private static readonly bool kDeleteOnReview = false;
        // Exclude the default admin, myself, from being displayed
        private static readonly string[] kExcludedUsers = new string[] { "a8ea39f9-e1b1-4ecb-bc88-7d22f5d2b165" };

        #region User Management
        // GET: AdminTools/UserManagement
        [Authorize(Roles = "staff")]
        public ActionResult UserManagement()
        {
            return View(model: GetUserPage(new UserManagementFilter()));
        }

        [HttpPost]
        [Authorize(Roles = "staff")]
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

        // AdminTools/UpdateUser
        [HttpPost]
        [Authorize(Roles = "staff")]
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

        [HttpDelete]
        [Authorize(Roles = "staff")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            bool success = await Task.Run<bool>(() =>
            {
                bool userDeleted = true;
                using (ApplicationDbContext context = ApplicationDbContext.Create())
                {
                    // Try to find the user to delete
                    ApplicationUser userToDelete = context.Users.SingleOrDefault(x => x.Id == id);

                    // If the user was found,
                    if(userToDelete != null)
                    {
                        // Delete the user
                        context.Users.Remove(userToDelete);
                        // Save the changes
                        context.SaveChanges();
                    } 
                    else
                    {
                        userDeleted = false;
                    }

                    return userDeleted;
                }
            });

            return Json(new { success = success });
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

        [NonAction]
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
                if (request != null)
                {
                    result = View(model: DatabaseRequestViewModel.FromModel(request));
                }
            }
             
            return result;
        }

        // TODO: Optimize this to use a separate thread
        [HttpPost]
        public ActionResult ViewRequest(RequestReviewViewModel request)
        {
            ActionResult result = View("Error");
            if (request != null)
            {
                DatabaseRequest fromDatabase;
                // Get the data from the request database
                using (ApplicationDbContext context = ApplicationDbContext.Create())
                {
                    fromDatabase = context.Requests.SingleOrDefault(x => x.Id == request.RequestId);
                    if (!fromDatabase.Reviewed)
                    {
                        // Only save changes to the database if the request was rejected or if the request was 
                        // approved and the database was updated
                        bool saveDatabase = !request.Approved || (request.Approved && CheckRequestsAndCommitChanges(request.NodeData, fromDatabase));
                        // If we should delete requests when they are reviewed,
                        if (kDeleteOnReview)
                        {
                            // Delete the request
                            context.Requests.Remove(fromDatabase);
                        }
                        else
                        {
                            // Update the information in the database
                            fromDatabase.Reviewed = true;
                            fromDatabase.ReviewedDate = DateTime.Now;
                            fromDatabase.Approved = request.Approved;
                            fromDatabase.Notes = request.Notes;
                            if (request.Approved)
                                fromDatabase.ApprovalDate = DateTime.Now;
                        }
                        // If the database action was successful,
                        if (saveDatabase)
                        {
                            // Save the changes to the database
                            context.SaveChanges();
                        }
                    }
                }

                result = RedirectToAction("DatabaseRequests");
            }

            return result;
        }

        [NonAction]
        public bool CheckRequestsAndCommitChanges(BasicNodeViewModel fromForm, DatabaseRequest fromDatabase)
        {
            bool result = true;
            
            //try
            //{
                DatabaseDriver systemDriver = new DatabaseDriver();
                if (fromDatabase.RequestType == DatabaseRequestType.Create)
                {
                    systemDriver.AddNode(fromForm.ToModel());
                }
                else if (fromDatabase.RequestType == DatabaseRequestType.Update)
                {
                    systemDriver.UpdateNode(fromForm.ToModel());
                }
                else if (fromDatabase.RequestType == DatabaseRequestType.Delete)
                {
                    systemDriver.DeleteNode(fromForm.Id);
                }
            //}
            //catch
            //{
            //    result = false;
            //}

            return result;
        }
        #endregion
    }
}