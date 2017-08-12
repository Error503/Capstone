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
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }
    }
}