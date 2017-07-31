using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MediaGraph.ViewModels.Edit;
using MediaGraph.Models.Util;

namespace MediaGraph.Controllers
{
    //[Authorize]
    public class RelEditController : Controller
    {
        [HttpGet]
        // GET: RelEdit/Index/Id
        public ActionResult Index(Guid? id)
        {
            ActionResult result = RedirectToActionPermanent("Index", "Home");
            // If the given id is null 
            if(id == null)
            {
                // If there is a node on the session,
                if(Session["createdNode"] != null)
                { 
                    // A node was found, return the view
                    result = View(model: new RelationshipDescriptionViewModel
                    {
                        SourceNode = Session["createdNode"] as NodeDescriptionViewModel,
                        RelatedMedia = new List<RelationshipEntryViewModel>(),
                        RelatedPeople = new List<RelationshipEntryViewModel>(),
                        RelatedCompanies = new List<RelationshipEntryViewModel>()
                    });
                } 
            }
            else
            {
                // There is an id, get the requested node
                // TODO: Get the node
                NodeDescriptionViewModel node = null;
                result = View(model: new RelationshipDescriptionViewModel
                {
                    SourceNode = node,
                    RelatedMedia = new List<RelationshipEntryViewModel>(),
                    RelatedPeople = new List<RelationshipEntryViewModel>(),
                    RelatedCompanies = new List<RelationshipEntryViewModel>()
                });
            }

            return result;
        }

        [HttpPost]
        public ActionResult Index(RelationshipDescriptionViewModel model)
        {
            // If the model state is invalid
            if(!ModelState.IsValid)
            {
                // Return the view
                return View(model: model);
            } 
            else
            {
                // Redirect to a different page
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult RelationshipEntry(NodeType sourceType, int targetType)
        {
            // If the source is a media type, then the source is the target
            int relType = (((int)sourceType & (int)NodeTypeExtensions.kGenericMedia) == (int)sourceType) ?
                (targetType << 16) & (int)sourceType : ((int)sourceType << 16) & targetType;
            // Return the partial view
            return PartialView("_RelationshipEntryPartialView", new RelationshipEntryViewModel());
        }
    }
}