using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaGraph.Code;
using Newtonsoft.Json;
using MediaGraph.Models;
using Neo4j.Driver.V1;

namespace MediaGraph.Controllers
{
    /// <summary>
    /// All action methods in this controller should return
    /// JSON string and be handled via Ajax.
    /// </summary>
    public class GraphController : Controller
    {
        private static IGraphDatabaseDriver databaseDriver = new Neo4jGraphDatabaseDriver();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index(GraphModel graphObject)
        {
            return View(JsonConvert.SerializeObject(graphObject));
        }

        [HttpPost]
        public ActionResult GetNodeData(Guid id)
        {
            FullNode node = null;
            // Get the node from the database
            INode result = databaseDriver.GetNode(id);
            // If the node exists,
            if(result != null)
            {
                node = FullNode.FromINode(result);
            }

            return Json(node);
        }

        [HttpPost]
        public ActionResult GetRelationshipData(string jsonString)
        {
            return Json(new { status = "NotImplemented" });
        }

        [HttpPost]
        public ActionResult GetGraphExtension(Guid id)
        {
            // Query the database, for the paths extending from the specified node
            return null;
        }
    }
}