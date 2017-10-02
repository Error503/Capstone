using MediaGraph.Models;
using MediaGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    //[Authorize(Roles = "admin")]
    public class DataReviewController : Controller
    {
        // GET: DataReview
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetRequests(FilterSettings filter)
        {
            DatabaseRequestPage response = null;
            using (ApplicationDbContext dbContext = ApplicationDbContext.Create())
            {
                // Filter the results
                // TODO: I bet I get a NullReference when filter.Submitter is not string.Empty
                IEnumerable<DatabaseRequest> filteredResults = from record in dbContext.Requests
                                                               where (string.IsNullOrEmpty(filter.Submitter) || record.Submitter.UserName.Equals(filter.Submitter)) &&
                                                                (filter.HasBeenReviewed == -1 || record.Reviewed && filter.HasBeenReviewed == 1 || !record.Reviewed && filter.HasBeenReviewed == 0) &&
                                                                (filter.HasBeenApproved == -1 || record.Approved && filter.HasBeenApproved == 1 || !record.Approved && filter.HasBeenApproved == 0) &&
                                                                (filter.SubmissionDate == DateTime.MinValue || record.SubmissionDate.CompareTo(filter.SubmissionDate) > 0)
                                                               orderby record.SubmissionDate
                                                               select record;
                // Get the specified page of results
                int resultCount = filteredResults.Count();
                int pageCount = (int)Math.Ceiling((double)resultCount / filter.ResultsPerPage);
                List<DatabaseRequestViewModel> requestsPage;
                // If there is a full page of results remaining
                int offset = (filter.Page - 1) * filter.ResultsPerPage;
                if (resultCount - offset >= filter.ResultsPerPage)
                {
                    // A full page exists
                    requestsPage = filteredResults.Skip(offset).Take(filter.ResultsPerPage).Select(e => DatabaseRequestViewModel.FromModel(e)).ToList();
                }
                else
                {
                    // Take the remaining entries
                    requestsPage = filteredResults.Skip(offset).Take(resultCount - offset).Select(e => DatabaseRequestViewModel.FromModel(e)).ToList();
                }

                // Create the response
                response = new DatabaseRequestPage
                {
                    CurrentPage = (offset / filter.ResultsPerPage) + 1,
                    TotalResults = resultCount,
                    TotalPages = pageCount,
                    Requests = requestsPage
                };
            }
            Response.StatusCode = 200;
            return Json(response);
        }

        [HttpGet]
        public ActionResult RequestView(string id)
        {
            DatabaseRequest request;
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Guid parsed;
                Guid.TryParse(id, out parsed);
                request = context.Requests.SingleOrDefault(x => x.Id == parsed);
            }

            return request == null ? View("Error") : View(model: request);
        }

        [HttpPost]
        public ActionResult Review(RequestReviewSubmission review)
        {
            throw new NotImplementedException();
        }
    }
}