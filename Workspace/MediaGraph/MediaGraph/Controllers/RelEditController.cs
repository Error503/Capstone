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
        public ActionResult Index(Guid id)
        {
            // TODO: Get the information for the node with the given GUID
            NodeDescriptionViewModel defaultNode = NodeEditController.nodes.SingleOrDefault(x => x.NodeId == id);
            if (defaultNode != null)
                return View(model: new RelationshipCollectionViewModel { SourceId = defaultNode.NodeId, SourceName = defaultNode.PrimaryName, SourceType = defaultNode.SimpleType });
            else
                return HttpNotFound("Could not find the node with the given id");
        }

        [HttpPost]
        public ActionResult Index(RelationshipCollectionViewModel model)
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
        public ActionResult RelationshipEntry(SimpleNodeType sourceType, int targetType)
        {
            // If the source is a media type, then the source is the target
            int relType = (((int)sourceType & (int)NodeType.Generic_Media) == (int)sourceType) ?
                (targetType << 16) & (int)sourceType : ((int)sourceType << 16) & targetType;
            // Return the partial view
            return PartialView("_RelationshipEntryPartialView", new RelationshipViewModel { RelationshipType = relType });
        }
    }
}