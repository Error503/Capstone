using MediaGraph.Models.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels.Graph
{
    public class TimelineDisplayViewModel
    {
        public string Id { get; set; }
        public NodeContentType DataType { get; set; }
        public string CommonName { get; set; }
        public long ReleaseDate { get; set; }
        public long DeathDate { get; set; }
    }
}