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
            return View("AltIndex");
        }

        [HttpPost]
        public ActionResult Index(NodeViewModel model)
        {
            ActionResult result = PartialView("_AcceptedPartial");

            if(model == null || !ModelState.IsValid)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                HttpContext.Response.StatusDescription = "Invalid model state";
                result = PartialView("_ValidationSummaryPartial", model);
            }
            else
            {
                // Give an id to the node
                model.Node.Id = Guid.NewGuid();
                // Create the DatabaseRequest
                DatabaseRequest request = new DatabaseRequest
                {
                    Id = Guid.NewGuid(),
                    SubmissionDate = DateTime.Now,
                    NodeData = JsonConvert.SerializeObject(model),
                    Reviewer = null,
                    Reviewed = false,
                    ReviewedDate = null,
                    ApprovalDate = null,
                    Approved = false,
                    Notes = null
                };
                // The model state is valid, accept the request to the database requests table
                using (ApplicationDbContext context = ApplicationDbContext.Create())
                {
                    request.Submitter = context.Users.Single(u => u.UserName == User.Identity.Name);
                    context.Requests.Add(request);
                    context.SaveChanges();
                }
            }

            return result;
        }

        private ActionResult Accpeted(NodeViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public NodeViewModel GetNode(Guid id)
        {
            // Get the node from the Neo4j database
            throw new NotImplementedException();
        }

        [HttpGet]
        public ActionResult GetInformation(string type)
        {
            ActionResult result = Json(new { msg = $"Invalid type {type}" }, JsonRequestBehavior.AllowGet);
            if(type == "company")
            {
                result = PartialView("_CompanyInformationPartial");
            }
            else if(type == "media")
            {
                result = PartialView("_MediaInformationPartial");
            }
            else if(type == "person")
            {
                result = PartialView("_PersonInformationPartial");
            } 
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return result;
        }
    }
}