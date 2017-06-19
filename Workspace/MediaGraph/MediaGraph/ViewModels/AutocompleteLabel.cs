using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.ViewModels
{
    [JsonObject]
    public class AutocompleteLabel
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}