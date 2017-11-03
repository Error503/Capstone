using System.Web;
using System.Web.Optimization;

namespace MediaGraph
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/core").Include(
                "~/Scripts/vis/vis.min.js",
                "~/Scripts/materialize/materialize.min.js",
                "~/Scripts/myScripts/menu.js"));
            bundles.Add(new ScriptBundle("~/bundles/validation").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/myScripts/validation-adapter.js"));
            bundles.Add(new ScriptBundle("~/bundles/visualization").Include(
                "~/Scripts/myScripts/graph/graph-controller.js",
                "~/Scripts/myScripts/graph/network-display-controller.js",
                "~/Scripts/myScripts/graph/timeline-display-controller.js"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Create the style bundles
            bundles.Add(new StyleBundle("~/style-bundles/core").Include(
                "~/Content/materialize/materialize.min.css",
                "~/Content/Site.css"));
            bundles.Add(new StyleBundle("~/style-bundles/visualization").Include(
                "~/Scripts/vis/vis.min.css",
                "~/Content/graphStyle.css"));
        }
    }
}
