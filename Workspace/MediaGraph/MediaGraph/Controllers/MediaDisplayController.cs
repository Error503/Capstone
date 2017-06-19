using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4j.Driver.V1;
using MediaGraph.Code;
using MediaGraph.ViewModels;
using MediaGraph.Models.Util;

namespace MediaGraph.Controllers
{
    public class MediaDisplayController : Controller
    {
        // Things to ask
        // Is it better to denormalize the names array - use individual fields with indices -
        // instead of using the array of names.
        // May be able to get around that with execution hints in Neo4j


        // GET: MediaDisplay
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewInfo()
        {
            return View(model: "{'key':7}");
        }

        [HttpPost]
        public ActionResult Search(SearchViewModel searchInfo)
        {
            return RedirectToAction("Index");
        }
    }
}