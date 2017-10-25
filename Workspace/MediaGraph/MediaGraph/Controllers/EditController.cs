using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    [Authorize]
    public class EditController : Controller
    {
        [HttpGet]
        // GET: Edit/Index/
        public ActionResult Index(string id)
        {
            ActionResult result = View("Error");
            if(!string.IsNullOrWhiteSpace(id))
            {
                // Try to parse the Guid string
                Guid parsedId;
                if(Guid.TryParse(id, out parsedId))
                {
                    // The string was parsed, find the node
                    // We have been given an id of a node to edit
                    ViewBag.Title = "Edit Node Data";
                    BasicNodeViewModel viewModel = null;
                    // Get the node to edit
                    using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
                    {
                        BasicNodeModel model = driver.GetNodeAndRelationships(parsedId);
                        viewModel = BasicNodeViewModel.FromModel(model);
                    }
                    // Return a view
                    if(viewModel != null)
                    {
                        result = View(model: viewModel);
                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Response.StatusDescription = $"Invalid id value: {id}";
                }
            }
            else
            {
                // We are creating a new node
                ViewBag.Title = "Create New Node";
                result = View(model: new BasicNodeViewModel { Id = Guid.NewGuid(), ContentType = 0 });
            }
            return result;
        }

        [HttpPost]
        public ActionResult Index(BasicNodeViewModel model)
        {
            return CheckModelAndMakeRequest(model);
        }

        [HttpGet]
        public ActionResult Visualize()
        {
            return View(model: new BasicNodeViewModel { Id = Guid.NewGuid(), ContentType = 0 });
        }

        [HttpGet]
        public ActionResult AutocompleteSearch(NodeContentType type, string searchText)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();
            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                Dictionary<string, string> searchResult = driver.SearchForNodes(type, searchText);
                result = searchResult.Select(x => new Tuple<string, string>(x.Key, x.Value)).ToList();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private ActionResult CheckModelAndMakeRequest(BasicNodeViewModel model)
        {
            ActionResult result = View("Index", model);
            // If the model state is valid,
            if(ModelState.IsValid)
            {
                DatabaseRequestType requestType = 0;
                using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
                {
                    requestType = driver.GetNode(model.Id) != null ? DatabaseRequestType.Update : DatabaseRequestType.Add;
                }
                // Create the database request
                CreateDatabaseRequest(model, requestType);
                // Redirect to the accepted page
                result = RedirectToAction("Accepted", "Edit", null);
            }

            return result;
        }

        private void CreateDatabaseRequest(BasicNodeViewModel model, DatabaseRequestType type)
        {
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Get the user that submitted the request
                ApplicationUser submitter = context.Users.Single(x => x.UserName == User.Identity.Name);
                // Create the request
                DatabaseRequest request = new DatabaseRequest
                {
                    RequestType = type,
                    Id = Guid.NewGuid(),
                    SubmissionDate = DateTime.Now,
                    Submitter = submitter,
                    NodeDataType = model.ContentType,
                    NodeData = model.SerializeToContentType(),
                };
                // Add the request to the database
                context.Requests.Add(request);
                context.SaveChanges();
            }
        }

        // Called when a request is accepted
        public ActionResult Accepted()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FlagDeletion(Guid id)
        {
            string message = "An error occurred. The node was not flagged";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            BasicNodeModel toDelete = null;

            using(Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                toDelete = driver.GetNode(id);
            }

            // If the node with the given id is not null,
            if (toDelete != null)
            {
                DatabaseRequest request = new DatabaseRequest
                {
                    Id = Guid.NewGuid(),
                    RequestType = DatabaseRequestType.Delete,
                    SubmissionDate = DateTime.Now,
                    NodeDataType = toDelete.ContentType,
                    NodeData = JsonConvert.SerializeObject(toDelete),
                    Approved = false,
                    ApprovalDate = null,
                    Notes = null,
                    Reviewed = false,
                    ReviewedDate = null,
                    Reviewer = null,
                    ReviewerRefId = null
                };

                // Add a deletion request,
                using (ApplicationDbContext context = ApplicationDbContext.Create())
                {
                    request.Submitter = context.Users.Single(u => u.UserName == User.Identity.Name);
                    context.Requests.Add(request);
                    context.SaveChanges();
                }

                message = "Node flagged for deletion";
                Response.StatusCode = (int)HttpStatusCode.Accepted;
            }
            else
            {
                message = "Could not find the specified node.";
                Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            return Json(new { message = message });
        }

        [HttpGet]
        public ActionResult GetInformation(int type)
        {
            NodeContentType contentType = (NodeContentType)type;
            ActionResult result = Json(new { msg = $"Invalid type {type}" }, JsonRequestBehavior.AllowGet);
            if(contentType == NodeContentType.Company)
            {
                result = PartialView("_CompanyInformationPartial", new CompanyNodeViewModel());
            }
            else if(contentType == NodeContentType.Media)
            {
                result = PartialView("_MediaInformationPartial", new MediaNodeViewModel());
            }
            else if(contentType == NodeContentType.Person)
            {
                result = PartialView("_PersonInformationPartial", new PersonNodeViewModel());
            } 
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return result;
        }
    }
}