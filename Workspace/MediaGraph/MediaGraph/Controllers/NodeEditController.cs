using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaGraph.ViewModels;
using MediaGraph.Models;
using MediaGraph.Code;
using MediaGraph.ViewModels.Edit;
using System.Threading.Tasks;

namespace MediaGraph.Controllers
{
    //[Authorize]
    public class NodeEditController : Controller
    {
        private static IGraphDatabaseDriver databaseDriver = new Neo4jGraphDatabaseDriver();

        public static List<NodeDescriptionViewModel> nodes = new List<NodeDescriptionViewModel>();

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
                // TODO: Get the node with the specified id
                NodeDescriptionViewModel toEdit = nodes.SingleOrDefault(x => x.NodeId == id);
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

        [HttpPost]
        // POST: NodeEdit/Index
        public ActionResult Index(NodeDescriptionViewModel model)
        {
            // If the model state is invalid,
            if (!ModelState.IsValid)
            {
                // Return the view
                return View(model: model);
            }
            else
            {
                // TODO: Handle valid requests

                // Put the valid model into the testing collection
                nodes.Add(model);

                // Redirect the user
                return RedirectToAction("Index", "Home");
            }
        }
    }
}