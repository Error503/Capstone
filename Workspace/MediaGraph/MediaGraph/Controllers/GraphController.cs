using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Graph;
using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    /// <summary>
    /// All action methods in this controller should return
    /// JSON string and be handled via Ajax.
    /// </summary>
    public class GraphController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SearchPaths(string searchText, Guid? id)
        {
            GraphDataViewModel data = GetPaths(searchText, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetNodeInformation(Guid id)
        {
            BasicNodeModel model = new BasicNodeModel();
            if(id != Guid.Empty)
            {
                using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
                {
                    model = driver.GetNode(id);
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private GraphDataViewModel GetPaths(string searchText, Guid? id)
        {
            // Get the paths from the database
            List<IPath> paths = new List<IPath>();
            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                if(id == null || id == Guid.Empty)
                {
                    paths = driver.GetPaths(searchText.ToLower());
                }
                else
                {
                    paths = driver.GetPaths(id.Value);
                }
            }

            return ConvertFromPaths(paths);
        }

        private GraphDataViewModel ConvertFromPaths(List<IPath> paths)
        {
            GraphDataViewModel result = new GraphDataViewModel();
            if(paths.Count > 0)
            {
                GraphNodeViewModel sourceNode = null;
                List<GraphNodeViewModel> relatedNodes = new List<GraphNodeViewModel>();
                // Create the source node
                sourceNode = new GraphNodeViewModel
                {
                    Id = paths[0].Start.Properties["id"].As<string>(),
                    DataType = (NodeContentType)Enum.Parse(typeof(NodeContentType), paths[0].Start.Labels[0]),
                    CommonName = paths[0].Start.Properties["commonName"].As<string>(),
                    ReleaseDate = paths[0].Start.Properties.ContainsKey("releaseDate") ? DateTime.Parse(paths[0].Start.Properties["releaseDate"].As<string>()).Ticks : 0
                };

                foreach (IPath p in paths)
                {
                    // Since I am only working with one other node in the path, I should be able to use the end node
                    relatedNodes.Add(new GraphNodeViewModel
                    {
                        Id = p.End.Properties["id"].As<string>(),
                        DataType = (NodeContentType)Enum.Parse(typeof(NodeContentType), p.End.Labels[0]),
                        CommonName = p.End.Properties["commonName"].As<string>(),
                        ReleaseDate = p.End.Properties.ContainsKey("releaseDate") ? DateTime.Parse(paths[0].End.Properties["releaseDate"].As<string>()).Ticks : 0,
                        RelationType = p.Relationships[0].Type,
                        Roles = p.Relationships[0].Properties["roles"].As<List<string>>()
                    });
                }

                result = new GraphDataViewModel { Source = sourceNode, RelatedNodes = relatedNodes };
            }

            return result;
        }
    }
}