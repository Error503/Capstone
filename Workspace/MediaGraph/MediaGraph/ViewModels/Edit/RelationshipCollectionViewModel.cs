using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MediaGraph.ViewModels.Edit
{
    [JsonObject]
    public class RelationshipCollectionViewModel
    {
        [JsonProperty("sourceId")]
        public Guid SourceId { get; set; }
        [JsonProperty("sourceType")]
        [Display(Name = "Source Content Type")]
        public SimpleNodeType SourceType { get; set; }
        [JsonProperty("sourceName")]
        [Display(Name = "Source Name")]
        public string SourceName { get; set; }

        [JsonProperty("media")]
        [Display(Name = "Related Media")]
        public IEnumerable<RelationshipViewModel> RelatedMedia { get; set; }

        [JsonProperty("people")]
        [Display(Name = "Related People")]
        public IEnumerable<RelationshipViewModel> RelatedPeople { get; set; }

        [JsonProperty("companies")]
        [Display(Name = "Related Companies")]
        public IEnumerable<RelationshipViewModel> RelatedCompanies { get; set; }
    }
}