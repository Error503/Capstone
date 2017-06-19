using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MediaGraph.ViewModels.Edit
{
    [JsonObject]
    public class RelationshipViewModel
    {
        [Required]
        [JsonProperty("targetId")]
        public Guid TargetId { get; set; }

        [Required]
        [JsonProperty("relType")]
        public int RelationshipType { get; set; }

        [Required]
        [JsonProperty("roles")]
        public string Roles { get; set; }
    }
}