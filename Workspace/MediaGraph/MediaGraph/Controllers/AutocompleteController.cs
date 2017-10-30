using MediaGraph.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    // We won't want to cache the results of this controller
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class AutocompleteController : Controller
    {
        [HttpGet]
        public JsonResult Index(string t)
        {
            List<Tuple<string, string>> results = new List<Tuple<string, string>>();
            using (Neo4jGraphDatabaseDriver driver = new Neo4jGraphDatabaseDriver())
            {
                Dictionary<string, string> matches = driver.SearchForNodes(t);
                results = matches.Select(x => new Tuple<string, string>(x.Key, x.Value)).ToList();
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}