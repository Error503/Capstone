using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MediaGraph.Models.Util;

namespace MediaGraph.ViewModels.Edit
{
    public class RelationshipDescriptionViewModel : IValidatableObject
    {
        public NodeDescriptionViewModel SourceNode { get; set; }

        public IEnumerable<RelationshipEntryViewModel> RelatedMedia { get; set; }
        public IEnumerable<RelationshipEntryViewModel> RelatedPeople { get; set; }
        public IEnumerable<RelationshipEntryViewModel> RelatedCompanies { get; set; }

        /// <summary>
        /// Performs validation on the relationship definitions.
        /// </summary>
        /// <param name="validationContext">The validation context to use</param>
        /// <returns>A collection of validation errors</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}