using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using MediaGraph.Models.Util;
using System.Threading.Tasks;

namespace MediaGraph.ViewModels.Edit
{
    [Obsolete]
    public class NodeDescriptionViewModel : IValidatableObject
    {
        #region Properties
        #region Universal Required Properties
        public Guid NodeId { get; set; }

        [Display(Name = "Content Type")]
        public NodeType ContentType { get; set; }

        public DateTime? Date { get; set; } = null;

        [Display(Name = "Common English Name")]
        public string PrimaryName { get; set; }
        #endregion

        #region Optional Properties
        [Display(Name = "Other Names")]
        public string AlternateNamesString { get; set; } = null;

        [Display(Name = "Franchise")]
        public string Franchise { get; set; } = null;

        [Display(Name = "Genres")]
        public long Genres { get; set; } = 0;
        #endregion
        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            // Check the Content Type
            if (ContentType == 0)
                errors.Add(new ValidationResult("A content type must be specified", new string[] { "ContentType" }));

            // Check if a primary name was given
            if (string.IsNullOrWhiteSpace(PrimaryName))
            {
                errors.Add(new ValidationResult("A primary name must be provided", new string[] { "PrimaryName" }));
            } 
            else
            {
                // Trim the name
                PrimaryName = PrimaryName.Trim();
            }

            // If this node is a media node
            if (ContentType != 0 && (ContentType & NodeTypeExtensions.kGenericMedia) == ContentType)
            {
                // Validate as a media object
                ValidateAsMediaNode(validationContext, ref errors);
            }
            else
            {
                // This is not a media node, ensure that values of Franchise and Genres are defaults
                Franchise = null;
                Genres = 0;
            }

            // Give the node a GUID if it needs one,
            if (NodeId == Guid.Empty && errors.Count == 0)
                NodeId = Guid.NewGuid();

            return errors;
        }

        /// <summary>
        /// Helper method that performs validation for media nodes.
        /// </summary>
        /// <param name="context">The validation context</param>
        /// <param name="errors">The collection of validation errors</param>
        private void ValidateAsMediaNode(ValidationContext context, ref List<ValidationResult> errors)
        { 
            // Check the release date
            if (Date == null)
                errors.Add(new ValidationResult("Media objects must have a valid release date", new string[] { "Date" }));
            // Check the franchise name, set it to null if it is a bad value
            Franchise = string.IsNullOrWhiteSpace(Franchise) ? null : Franchise;
            // TODO: Verify genres?
        }
    }
}