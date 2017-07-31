using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels;
using MediaGraph.ViewModels.Edit;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    //[Authorize]
    public class NodeEditController : Controller
    {
        private static IGraphDatabaseDriver databaseDriver = new Neo4jGraphDatabaseDriver();

        public static List<NodeDescriptionViewModel> nodes = new List<NodeDescriptionViewModel>();

        #region Node Edit
        [HttpGet]
        // GET: NodeEdit/Index/<GUID>
        public ActionResult Index(Guid? id)
        { 
            if(id == null)
            {
                return View(new NodeDescriptionViewModel());
            }
            else
            {
                // Find the node to edit
                NodeDescription toEdit = databaseDriver.GetNode(id.As<Guid>());
                if(toEdit != null)
                {
                    // The node was found, return it
                    return View(toEdit);
                }
                else
                {
                    return HttpNotFound("Could not find a node by that Id");
                }
            }
        }

        [HttpGet]
        public ActionResult EditNode()
        {
            return View();
        }

        [HttpPost]
        // POST: NodeEdit/CreateNode
        public ActionResult CreateNode(NodeDescriptionViewModel model)
        {
            // If the model state is invalid,
            if (!ModelState.IsValid)
            {
                // Return the view
                return View("Index", model: model);
            }
            else
            {
                // Attempt to put the node in the database,
                //if (databaseDriver.AddNode(NodeDescription.FromViewModel(model)))
                    // The creation was successful, redirect
                    // Add the view model data to the session storage
                    Session.Add("createdNode", model);
                    return RedirectToAction("RequestAccepted", model.NodeId);
            }
        }

        public ActionResult RequestAccepted()
        {
            return View(Session["createdNode"].As<NodeDescriptionViewModel>());
        }
        #endregion
    }
}