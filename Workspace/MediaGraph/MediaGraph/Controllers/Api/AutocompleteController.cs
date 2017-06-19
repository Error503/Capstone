using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MediaGraph.ViewModels;

namespace MediaGraph.Controllers
{
    [RoutePrefix("api/autocomplete")]
    public class AutocompleteController : ApiController
    {
        private static AutocompleteLabel[] testValues = new AutocompleteLabel[] 
        {
            new AutocompleteLabel { Label = "Compile Heart" , Value = 43 },
            new AutocompleteLabel { Label = "Bandai Namco", Value = 100 },
            new AutocompleteLabel { Label = "Namco", Value = 520932 },
            new AutocompleteLabel { Label = "Square Enix", Value = 53 },
            new AutocompleteLabel { Label = "Koei Techimo", Value = 532 },
            new AutocompleteLabel { Label = "Gust", Value = 2 },
            new AutocompleteLabel { Label = "Idea Factory", Value = 1 }
        };

        [HttpGet]
        [Route("Search")]
        public IEnumerable<AutocompleteLabel> Search(int field, string text)
        {
            // Normalize the text input
            text = text.ToLower();
            return testValues.Where(x => x.Label.ToLower().Contains(text)).ToList();
        }

        [HttpGet]
        [Route("SearchAsync")]
        // GET: api/autocomplete/searchasync?field=1&text=val
        public async Task<IEnumerable<AutocompleteLabel>> SearchAsync(int field, string text)
        {
            // Normalize the text input
            text = text.ToLower();
            IEnumerable<AutocompleteLabel> values = testValues.Where(x => x.Label.ToLower().Contains(text));
            return await Task.FromResult(values);
        }
    }
}