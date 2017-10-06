using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels;
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
                // We have been given an id of a node to edit
                ViewBag.Title = "Edit Node Data";
                // TODO: Get the Node information from the database
            }
            else
            {
                // We are creating a new node
                ViewBag.Title = "Create New Node";
                result = View(model: new BasicNodeViewModel { Id = Guid.NewGuid(), ContentType = "" });
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
                if(modelToEdit.ContentType == "company")
                {
                    modelToEdit = JsonConvert.DeserializeObject<CompanyNodeViewModel>(request.NodeData) as CompanyNodeViewModel;
                }
                else if(modelToEdit.ContentType == "media")
                {
                    modelToEdit = JsonConvert.DeserializeObject<MediaNodeViewModel>(request.NodeData) as MediaNodeViewModel;
                }
                else if(modelToEdit.ContentType == "person")
                {
                    modelToEdit = JsonConvert.DeserializeObject<PersonNodeViewModel>(request.NodeData) as PersonNodeViewModel;
                }
            }

            return View("Index", modelToEdit);
        }

        [HttpPost]
        public ActionResult SubmitCompany(CompanyNodeViewModel model)
        { 
            ActionResult result = View("Error");
            // The model state is valid
            if (ModelState.IsValid)
            {
                // TODO: Add to the database requests database - If a node exists in the Neo4j database then it is an update request 
                result = RedirectToAction("Accepted");
            }
            else
            {
                // Return the view via the edit page
            }

            return result;
        }

        [HttpPost]
        public ActionResult SubmitMedia(MediaNodeViewModel model)
        {
            if(ModelState.IsValid)
            {

            }
            else
            {
                // Return the view via the edit page
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
            }

            return result;
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
                    //RequestType = "Delete",
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
        public ActionResult GetInformation(string type)
        {
            ActionResult result = Json(new { msg = $"Invalid type {type}" }, JsonRequestBehavior.AllowGet);
            if(type == "company")
            {
                result = PartialView("_CompanyInformationPartial", new CompanyNodeViewModel());
            }
            else if(type == "media")
            {
                result = PartialView("_MediaInformationPartial", new MediaNodeViewModel());
            }
            else if(type == "person")
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