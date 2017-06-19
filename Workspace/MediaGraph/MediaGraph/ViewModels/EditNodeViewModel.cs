using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MediaGraph.Models;
using MediaGraph.Models.Util;

namespace MediaGraph.ViewModels
{
    public class EditNodeViewModel : IValidatableObject
    {
        public Guid Id { get; set; }
        [Display(Name = "Content Label")]
        public NodeType Labels { get; set; }
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }
        [Display(Name = "Names")]
        public IEnumerable<string> Names { get; set; }

        // Media-only data
        public string Franchise { get; set; }
        public IEnumerable<string> Genres { get; set; }

        public IDictionary<GuidStringKey, RelationshipType> DirectRelationships { get; set; }

        /// <summary>
        /// Constructs an empty EditNodeViewModel.
        /// </summary>
        public EditNodeViewModel()
        {
            Names = new List<string>();
            Genres = new List<string>();
            DirectRelationships = new Dictionary<GuidStringKey, RelationshipType>();
        }

        /// <summary>
        /// Constructs a new EditNodeViewModel from the given EditNode.
        /// </summary>
        /// <param name="node">The EditNode from which to construct the view model</param>
        public EditNodeViewModel(EditNode node)
        {
            Id = node.NodeData.Id;
            Labels = node.NodeData.Labels;
            Names = node.NodeData.Properties["names"] as List<string>;
            object val;
            DateTime temp;
            // Try to get the release date value
            if(node.NodeData.Properties.TryGetValue("releaseDate", out val) && DateTime.TryParse(val as string, out temp))
            {
                // The value was found and parsed successfully
                ReleaseDate = temp;
            }

            // Check for other information
            if((Labels & NodeType.Generic_Media) == Labels)
            {
                // This node is some form of media, so get media information
                Franchise = node.NodeData.Properties["franchise"] as string;
                Genres = node.NodeData.Properties["genres"] as List<string>;
            }

            // Set the relationship values
            DirectRelationships = node.DirectRelationships;
        }

        /// <summary>
        /// Implementation of the Validate method for model validation.
        /// </summary>
        /// <param name="validationContext">The validation context to use</param>
        /// <returns>A collection of validation errors if any exist</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}