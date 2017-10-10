using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaGraph.ViewModels;
using MediaGraph.Models;
using MediaGraph.Code;
using MediaGraph.Models.Util;
using MediaGraph.Models.Component;
using System.Net;
using Neo4j.Driver.V1;

namespace MediaGraph.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string id)
        {
            ActionResult result = Json(new { msg = "Failed to process request" }, JsonRequestBehavior.AllowGet);

            if(!string.IsNullOrWhiteSpace(id))
            {
                try
                {
                    BasicNodeModel model = null;
                    using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
                    {
                        model = driver.GetNode(Guid.Parse(id));
                    }

                    result = Json(new { msg = "Found node", data = model }, JsonRequestBehavior.AllowGet);
                }
                catch(Exception e)
                {
                    result = Content(e.Message + "\n" + e.StackTrace + "\n\n" + e.InnerException.Message + "\n" + e.InnerException.StackTrace);
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }

            return result;
        }
    }
}