using System.Web.Optimization;
using cts.commons.web;

namespace softWrench.sW4.Web {
    public class WebBundleConfig :IBundleConfigProvider {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725

        public void PopulateStyleBundles(BundleCollection bundles) {

            // from bower styles
            bundles.Add(new StyleBundle("~/Content/vendor/css").Include(
                    "~/Content/vendor/css/*.css"
            ));
          
            // customized vendor styles
            bundles.Add(new StyleBundle("~/Content/customVendor/css").Include(
               "~/Content/customVendor/css/bootstrap-combobox.css",
               "~/Content/customVendor/css/bootstrap-select.css",
               "~/Content/customVendor/css/submenu.css",
               "~/Content/customVendor/css/typeahead.js-bootstrap.css",
               "~/Content/customVendor/css/bootstrap-multiselect.css"
               ));

            // from bower fonts
            bundles.Add(new StyleBundle("~/Content/fonts").Include(
                "~/Content/fonts/font.css"
                ));
        }

        public void PopulateScriptBundles(BundleCollection bundles) {

            // from bower scripts
            bundles.Add(new ScriptBundle("~/Content/vendor/scripts").Include(
                "~/Content/vendor/scripts/*.js"
            ));

            // customized vendor scripts
            bundles.Add(new ScriptBundle("~/Content/vendor/scripts").Include(
                "~/Content/customVendor/scripts/*.js"
            ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin").Include(
                "~/Content/Scripts/client/signin/signin.js"
                ));

            // app scripts
            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application")
                .IncludeDirectory("~/Content/Scripts/client/crud", "*.js", true)
                .Include(
                "~/Content/Scripts/client/services/*.js",
                "~/Content/Scripts/client/*.js",
                "~/Content/Scripts/client/adminresources/*.js",
                "~/Content/Scripts/client/directives/*.js",
                "~/Content/Scripts/client/directives/menu/*.js")
                .IncludeDirectory("~/Content/Templates/commands", "*.js", true)
                .IncludeDirectory("~/Content/modules", "*.js", true));
            
        }

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }
    }
}