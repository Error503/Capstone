using MediaGraph.Code;
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
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            // Add a custom model binder for DateTimes
            //CustomDateBinder binder = new CustomDateBinder();
            //ModelBinders.Binders.Add(typeof(DateTime), binder);
            //ModelBinders.Binders.Add(typeof(DateTime?), binder);
        }
    }
}
