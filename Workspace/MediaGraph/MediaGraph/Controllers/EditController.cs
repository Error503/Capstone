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
        private static IGraphDatabaseDriver databaseDriver = new Neo4jGraphDatabaseDriver();

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
                    // TODO: Get the node data from the database
                    //NodeRelationshipModel nodeAndRelationships = databaseDriver.GetNodeAndDirectRelationships(parsedId);
                    BasicNodeViewModel nodeToEdit = new BasicNodeViewModel();
                    result = View(model: nodeToEdit);
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

        // This action is called to edit a node from a database request
        [HttpGet]
        [Authorize(Roles = "admin,staff")]
        public ActionResult EditRequest(Guid id) 
        {
            BasicNodeViewModel modelToEdit = null;
            // Get the node to edit from the databsae rquests database
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Find the request
                DatabaseRequest request = context.Requests.Single(x => x.Id == id);
                // Get the node to edit
                modelToEdit = JsonConvert.DeserializeObject<BasicNodeViewModel>(request.NodeData) as BasicNodeViewModel;
                // TODO: By adding the content type to the database request I can simplify this and not have to deserialize twice
                // Check the content type
                if(modelToEdit.ContentType == NodeContentType.Company)
                {
                    modelToEdit = JsonConvert.DeserializeObject<CompanyNodeViewModel>(request.NodeData) as CompanyNodeViewModel;
                }
                else if(modelToEdit.ContentType == NodeContentType.Media)
                {
                    modelToEdit = JsonConvert.DeserializeObject<MediaNodeViewModel>(request.NodeData) as MediaNodeViewModel;
                }
                else if(modelToEdit.ContentType == NodeContentType.Person)
                {
                    modelToEdit = JsonConvert.DeserializeObject<PersonNodeViewModel>(request.NodeData) as PersonNodeViewModel;
                }
            }

            return View("Index", modelToEdit);
        }

        [HttpPost]
        public ActionResult SubmitCompany(CompanyNodeViewModel model)
        {
            ActionResult result = View("Index", model);
            // The model state is valid
            if (ModelState.IsValid)
            {
                // Create the database request
                CreateDatabaseRequest(model);
                // Redirect to the accepted page
                result = RedirectToAction("Accepted");
            }

            return result;
        }

        [HttpPost]
        public ActionResult SubmitMedia(MediaNodeViewModel model)
        {
            ActionResult result = View("Index", model);
            if(ModelState.IsValid)
            {
                // Create the database request
                CreateDatabaseRequest(model);
                // Redirect to the accepted page
                result = RedirectToAction("Accepted");  
            }

            return View();
        }

        [HttpPost]
        public ActionResult SubmitPerson(PersonNodeViewModel model)
        {
            ActionResult result = View("Index", model);

            // If the model state is valid
            if(ModelState.IsValid)
            {
                // Create the database request
                CreateDatabaseRequest(model);
                // Redirect to the accepted page
                result = RedirectToAction("Accepted");
            }

            return result;
        }

        private void CreateDatabaseRequest(BasicNodeViewModel node)
        {
            RequestType requestType = databaseDriver.GetNode(node.Id) != null ? RequestType.Update : RequestType.Add;
            using (ApplicationDbContext context = ApplicationDbContext.Create())
            {
                // Get the user that submitted the request
                ApplicationUser submitter = context.Users.Single(x => x.UserName == User.Identity.Name);
                // Create the request
                DatabaseRequest request = new DatabaseRequest
                {
                    //RequestType = requestType,
                    Id = Guid.NewGuid(),
                    SubmissionDate = DateTime.Now,
                    Submitter = submitter,
                    //NodeContentType = node.ContentType,
                    NodeData = node.SerializeToContentType(),
                };
                // Add the request to the database
                context.Requests.Add(request);
                context.SaveChanges();
            }
        }

        // Called when a request is accepted
        private ActionResult Accepted()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FlagDeletion(Guid id)
        {
            string message = "An error occurred. The node was not flagged";
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // If the node with the given id is not null,
            if (databaseDriver.GetNode(id) != null)
            {
                DatabaseRequest request = new DatabaseRequest
                {
                    Id = Guid.NewGuid(),
                    SubmissionDate = DateTime.Now,
                    //RequestType = RequestType.Delete,
                    NodeData = JsonConvert.SerializeObject(new { id = id }),
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