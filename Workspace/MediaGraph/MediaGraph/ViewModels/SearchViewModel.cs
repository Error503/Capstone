using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MediaGraph.Models;
using MediaGraph.Models.Util;

namespace MediaGraph.ViewModels
{
    public class SearchViewModel
    {
        public string Title { get; set; }
        public NodeType Label { get; set; }
    }
}