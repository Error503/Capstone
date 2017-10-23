using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.Models.Component;
using MediaGraph.ViewModels.Edit;
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
        public ActionResult GetNetworkInformation(string searchText, Guid? id)
        {
            GraphDataViewModel data = GetPaths(searchText, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTimelineInformation(DateTime start, DateTime end)
        {
            return Json(GetTimelineElements(start, end), JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public ActionResult GetEdgeInformation(Guid source, Guid target)
        {
            GraphEdgeViewModel viewModel = new GraphEdgeViewModel();
            if(source != Guid.Empty && target != Guid.Empty)
            {
                using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
                {
                    IRecord record = driver.GetRelationship(source, target);

                    if(record != null)
                    {
                        viewModel = new GraphEdgeViewModel
                        {
                            SourceName = record[0].As<string>(),
                            TargetName = record[2].As<string>(),
                            Roles = record[1].As<List<string>>()
                        };
                    }
                }
            }

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        // TODO: This method should be moved out into a separate controller
        [HttpGet]
        public ActionResult SearchForNodes(string text)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                Dictionary<string, string> matches = driver.SearchForNodes(text);
                result = matches.Select(x => new Tuple<string, string>(x.Key, x.Value)).ToList();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Network Display Helper Methods
        private GraphDataViewModel GetPaths(string searchText, Guid? id)
        {
            GraphDataViewModel result = new GraphDataViewModel();
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

                if(paths.Count == 0)
                {
                    BasicNodeModel model = id == null || id == Guid.Empty ? driver.GetNode(searchText) : driver.GetNode(id.Value);

                    if(model != null)
                    {
                        result.Source = new GraphNodeViewModel
                        {
                            Id = model.Id.ToString(),
                            CommonName = model.CommonName,
                            DataType = model.ContentType
                        };
                        result.RelatedNodes = new List<GraphNodeViewModel>();
                    }
                }
                else
                {
                    result = ConvertFromPaths(paths);
                }
            }

            return result;
        }

        private GraphDataViewModel ConvertFromPaths(List<IPath> paths)
        {
            GraphDataViewModel result = new GraphDataViewModel();
            GraphNodeViewModel sourceNode = null;
            List<GraphNodeViewModel> relatedNodes = new List<GraphNodeViewModel>();
            // Create the source node
            sourceNode = new GraphNodeViewModel
            {
                Id = paths[0].Start.Properties["id"].As<string>(),
                DataType = (NodeContentType)Enum.Parse(typeof(NodeContentType), paths[0].Start.Labels[0]),
                CommonName = paths[0].Start.Properties["commonName"].As<string>(),
            };

            foreach (IPath p in paths)
            {
                // Since I am only working with one other node in the path, I should be able to use the end node
                relatedNodes.Add(new GraphNodeViewModel
                {
                    Id = p.End.Properties["id"].As<string>(),
                    DataType = (NodeContentType)Enum.Parse(typeof(NodeContentType), p.End.Labels[0]),
                    CommonName = p.End.Properties["commonName"].As<string>(),
                    RelationType = p.Relationships[0].Type,
                    Roles = p.Relationships[0].Properties["roles"].As<List<string>>()
                });
            }

            result = new GraphDataViewModel { Source = sourceNode, RelatedNodes = relatedNodes };

            return result;
        }
        #endregion

        #region Timeline Display Helper Methods
        private List<TimelineDisplayViewModel> GetTimelineElements(DateTime start, DateTime end)
        {
            List<TimelineDisplayViewModel> result = new List<TimelineDisplayViewModel>();

            List<BasicNodeModel> nodes;
            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                nodes = driver.GetNodesBetweenDates(start, end);
            }

            // Convert the nodes to TimelineDisplayViewModel objects
            foreach (BasicNodeModel n in nodes)
            {
                result.Add(new TimelineDisplayViewModel
                {
                    Id = n.Id.ToString(),
                    DataType = n.ContentType,
                    CommonName = n.CommonName,
                    ReleaseDate = n.ReleaseDate.HasValue ? n.ReleaseDate.Value : -1,
                    DeathDate = n.DeathDate.HasValue ? n.DeathDate.Value : -1
                });
            }

            return result;
        }
        #endregion
    }
}