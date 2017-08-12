using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels;
using MediaGraph.ViewModels.Edit;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    //[Authorize]
    public class EditController : Controller
    {
        private static IGraphDatabaseDriver databaseDriver = new Neo4jGraphDatabaseDriver();

        [HttpGet]
        // GET: Edit/Index/
        public ActionResult Index()
        {
            return View();
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

            return result;
        }

        private ActionResult Accpeted(NodeViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public NodeViewModel GetNode(Guid id)
        {
            // Access the database to get the node to edit
            throw new NotImplementedException();
        }
    }
}