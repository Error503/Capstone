using MediaGraph.Models.Component;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels.Graph
{
    public class GraphDataViewModel
    {
        // Source Id
        public GraphNodeViewModel Source { get; set; }
        // Related Nodes
        public IEnumerable<GraphNodeViewModel> RelatedNodes { get; set; }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class GraphNodeViewModel
    {
        public string Id { get; set; }
        public NodeContentType DataType { get; set; }
        public string CommonName { get; set; }

        // These will be null on the source node
        public string RelationType { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}