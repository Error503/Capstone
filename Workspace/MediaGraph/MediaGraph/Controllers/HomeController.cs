using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MediaGraph.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult AccountDeleted()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TestConnection()
        {
            List<AutocompleteRecord> results = new List<AutocompleteRecord>();
            using (AutocompleteDatabaseDriver driver = new AutocompleteDatabaseDriver())
            {
                results = driver.Search("text");
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}