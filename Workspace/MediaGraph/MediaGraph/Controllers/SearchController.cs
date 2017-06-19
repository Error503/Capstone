using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaGraph.ViewModels;
using MediaGraph.Models;
using MediaGraph.Code;
using MediaGraph.Models.Util;

namespace MediaGraph.Controllers
{
    public class SearchController : Controller
    {
        private static DatabaseDriver driver = new DatabaseDriver();
        private static GraphDescriptor descriptor = new GraphDescriptor
        {
            Style = GraphStyle.Relationship,
            Nodes = new NodeDefinition[]
            {
                new NodeDefinition { Id="someID", Label = Models.Util.NodeType.Game, Location = new GraphPoint { X = 0, Y = 0 }, Names = new string[] {"Test1", "Test 1"} },
                new NodeDefinition { Id="otherID", Label = Models.Util.NodeType.Game, Location = new GraphPoint { X = 1, Y = -1 }, Names = new string[] {"Test2", "Test 2"} }
            },
            Relationships = new RelationshipDefinition[]
            {
                new RelationshipDefinition { StartNode = "someID", EndNode = "otherID", Label = RelationshipType.Multiple }
            }
        };

        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel searchQuery)
        {
            return RedirectToAction("Index", "Graph", "");
            //// If the model state is valid,
            //if(ModelState.IsValid)
            //{
            //    // Perform the search and redirect to the graph view
            //    return RedirectToAction("Index", "Graph", null);
            //}
            //else
            //{
            //    // Otherwise return the model
            //    return View(searchQuery);
            //}
        }

        public ActionResult GetAdvancedSearchFields()
        {
            return PartialView(viewName: "_AdvancedSearchFormPartial");
        }
    }
}