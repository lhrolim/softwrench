using System.Web.Optimization;
using cts.commons.web;
using softWrench.sW4.Web.Util.StaticFileLoad;

namespace softWrench.sW4.Web {
    public class WebBundleConfig :IBundleConfigProvider {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725

        public void PopulateStyleBundles(BundleCollection bundles) {

            // from bower styles
            bundles.Add(new StyleBundle("~/Content/vendor/css")
                .IncludeDirectory("~/Content/vendor/css/bootstrap/", "*.css")
                .IncludeDirectory("~/Content/vendor/css/font-awesome/", "*.css")
                .IncludeDirectory("~/Content/vendor/css/angular/", "*.css")
                );
          
            // customized vendor styles
            bundles.Add(new StyleBundle("~/Content/customVendor/css")
                .IncludeDirectory("~/Content/customVendor/css/", "*.css")
                );

            // from bower fonts
            bundles.Add(new StyleBundle("~/Content/fonts")
                .Include( "~/Content/fonts/font.css")
                );
        }

        public void PopulateScriptBundles(BundleCollection bundles) {

            // from bower scripts
            var vendorBundle = new ScriptBundle("~/Content/vendor/scripts")
                // utils
                .IncludeDirectory("~/Content/vendor/scripts/utils/", "*.js")
                // jquery
                .Include(
                    "~/Content/vendor/scripts/jquery/jquery.js",
                    "~/Content/vendor/scripts/jquery/jquery-ui.js",
                    "~/Content/vendor/scripts//jquery-file-style.js",
                    "~/Content/vendor/scripts/jquery/jquery-file-download.js",
                    "~/Content/vendor/scripts/jquery/jquery-file-upload.js"
                )
                // bootstrap
                .Include(
                    "~/Content/vendor/scripts/bootstrap/bootstrap.js",
                    "~/Content/vendor/scripts/bootstrap/bootstrap-combobox.js",
                    "~/Content/vendor/scripts/bootstrap/bootstrap-datetimepicker.js",
                    "~/Content/vendor/scripts/bootstrap/bootstrap-multiselect.js",
                    "~/Content/vendor/scripts/bootstrap/bootbox.js"
                )
                // angular
                .Include(
                    "~/Content/vendor/scripts/angular/angular.js",
                    "~/Content/vendor/scripts/angular/angular-bindonce.js",
                    "~/Content/vendor/scripts/angular/angular-sanitize.js",
                    "~/Content/vendor/scripts/angular/angular-strap.js",
                    "~/Content/vendor/scripts/angular/angular-animate.js",
                    "~/Content/vendor/scripts/angular/angular-xeditable.js",
                    "~/Content/vendor/scripts/angular/angular-file-upload.js"
                );
            vendorBundle.Orderer = new PassthroughBundleOrderer(); // enforcing import order
            bundles.Add(vendorBundle);

            // customized vendor scripts
            bundles.Add(new ScriptBundle("~/Content/customVendor/scripts")
                .IncludeDirectory("~/Content/customVendor/scripts/", "*.js", true)
                );

            // app scripts
            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin").Include(
                "~/Content/Scripts/client/signin/signin.js"
                ));

            // app scripts
            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application")
                .IncludeDirectory("~/Content/Scripts/client/crud", "*.js", true)
                .IncludeDirectory("~/Content/Scripts/client/services/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/adminresources/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/directives/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/directives/menu/", "*.js")
                .IncludeDirectory("~/Content/Templates/commands", "*.js", true)
                .IncludeDirectory("~/Content/modules", "*.js", true));

        }

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }
    }
}