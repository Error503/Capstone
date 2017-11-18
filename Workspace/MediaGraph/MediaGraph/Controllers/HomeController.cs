using MediaGraph.Code;
using MediaGraph.Models;
using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            using (CassandraDriver driver = new CassandraDriver())
            {
                results = driver.Search("text");
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult TestInsertion(string id)
        {
            using (CassandraDriver driver = new CassandraDriver())
            {
                driver.AddNode(new Models.Component.BasicNodeModel
                {
                    Id = Guid.Parse(id),
                    CommonName = "Testing Entry",
                    OtherNames = new List<string> { "Testing Entry: Second Name", "Testing Entry: The third name" },
                    ContentType = Models.Component.NodeContentType.Company
                });
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> TestSearch(string text)
        {
            List<AutocompleteRecord> results = new List<AutocompleteRecord>();
            using (CassandraDriver driver = new CassandraDriver())
            {
                // Return the data
                results = await driver.SearchAsync(text);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult TestDelete(string id)
        {
            using(CassandraDriver driver = new CassandraDriver())
            {
                driver.DeleteNode(id);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}