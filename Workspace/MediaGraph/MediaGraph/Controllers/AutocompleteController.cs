using MediaGraph.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MediaGraph.Controllers
{
    // We won't want to cache the results of this controller
    [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
    public class AutocompleteController : Controller
    {
        [HttpGet]
        public async Task<JsonResult> Index(string t)
        {
            List<AutocompleteRecord> results = new List<AutocompleteRecord>();
            // Query the autocomplete database
            using (AutocompleteDatabaseDriver autocompleteDriver = new AutocompleteDatabaseDriver())
            {
                results = await autocompleteDriver.SearchAsync(t);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}