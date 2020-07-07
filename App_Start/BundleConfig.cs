using System.Web;
using System.Web.Optimization;

namespace TchOpenSource
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Content/fusion/js/jquery-min.js",
                        "~/Content/fusion/js/popper.min.js",
                        "~/Content/fusion/js/bootstrap.min.js",
                        "~/Content/fusion/js/owl.carousel.min.js",
                        "~/Content/fusion/js/wow.js",
                        "~/Content/fusion/js/jquery.nav.js",
                        "~/Content/fusion/js/scrolling-nav.js",
                        "~/Content/fusion/js/jquery.easing.min.js",
                        "~/Content/fusion/js/jquery.counterup.min.js",
                        "~/Content/fusion/js/waypoints.min.js",
                        "~/Content/fusion/js/main.js"));

            bundles.Add(new ScriptBundle("~/bundles/tinymce")
                        .Include("~/Scripts/tinymce/tinymce.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/fusion/css/bootstrap.min.css",
                      "~/Content/fusion/fonts/line-icons.css",
                      "~/Content/fusion/css/owl.carousel.min.css",
                      "~/Content/fusion/css/owl.theme.css",
                      "~/Content/fusion/css/magnific-popup.css",
                      "~/Content/fusion/css/nivo-lightbox.css",
                      "~/Content/fusion/css/animate.css",
                      "~/Content/fusion/css/main.css",
                      "~/Content/fusion/css/responsiveness.css"));
        }
    }
}
