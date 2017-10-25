using MediaGraph.Code;
using MediaGraph.ViewModels.AdminTools;
using MediaGraph.ViewModels.Edit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MediaGraph
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            // Add custom model binders
            ModelBinders.Binders.Add(typeof(BasicNodeViewModel), new NodeModelBinder());
            ModelBinders.Binders.Add(typeof(RequestReviewViewModel), new RequestReviewModelBinder());
        }
    }
}
