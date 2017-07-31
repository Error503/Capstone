using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MediaGraph.Models.Util;

namespace MediaGraph.ViewModels.Edit
{
    public class RelationshipEntryViewModel : IValidatableObject
    {
        public NodeType SourceType { get; set; }
        public NodeType TargetType { get; set; }

        public string RelationshipType { get; set; }
        public string Roles { get; set; }

        /// <summary>
        /// Validates the relationship entry.
        /// </summary>
        /// <param name="validationContext">The validation context to use</param>
        /// <returns>A collection of validation errors</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}