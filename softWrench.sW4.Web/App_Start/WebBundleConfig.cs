using System.Web.Optimization;
using cts.commons.web;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util.StaticFileLoad;

namespace softWrench.sW4.Web {
    public class WebBundleConfig : IBundleConfigProvider {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725

        public void PopulateStyleBundles(BundleCollection bundles) {
            if (ApplicationConfiguration.IsLocal()) {
                PopulateLocalStyleBundles(bundles);
                // from bower fonts
                bundles.Add(new StyleBundle(Bundles.Local.FontsStyles)
                    .Include("~/Content/fonts/font.css"));
            } else {
                PopulateDistributionStyleBundles(bundles);
            }

            
        }

        private void PopulateLocalStyleBundles(BundleCollection bundles) {
            // from bower styles
            var vendorBundle = new StyleBundle(Bundles.Local.VendorStyles)
                // bootstrap
                .Include("~/Content/vendor/css/bootstrap/bootstrap.css")
                .Include("~/Content/vendor/css/bootstrap/bootstrap-theme.css")
                .Include("~/Content/vendor/css/bootstrap/bootstrap-datetimepicker.css")
                .Include("~/Content/vendor/css/bootstrap/selectize.css")
                // font-awesome
                .IncludeDirectory("~/Content/vendor/css/font-awesome/", "*.css")
                // angular
                .IncludeDirectory("~/Content/vendor/css/angular/", "*.css")
                .Include("~/Content/vendor/css/angular/angular-ui-select.css")
                .Include("~/Content/vendor/css/angular/textAngular.css")
                .Include("~/Content/vendor/css/angular-ui-grid/ui-grid.css");
            vendorBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(vendorBundle);

            // customized vendor styles
            bundles.Add(new StyleBundle(Bundles.Local.CustomVendorStyles)
                .IncludeDirectory("~/Content/customVendor/css/", "*.css")
                );
        }

        private void PopulateDistributionStyleBundles(BundleCollection bundles) {
            // sass compiled and fonts already being distributed
            bundles.Add(new StyleBundle(Bundles.Distribution.VendorStyles)
                .IncludeDirectory("~/Content/dist/css", "*.css"));
        }

        public void PopulateScriptBundles(BundleCollection bundles) {
            var isLocal = ApplicationConfiguration.IsLocal();
            if (isLocal) {
                PopulateLocalScriptBundles(bundles);
            } else {
                PopulateDistributionScriptBundles(bundles);
            }

            // login script
            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin")
                .Include("~/Content/Scripts/client/signin/signin.js"));
        }

        private void PopulateLocalScriptBundles(BundleCollection bundles) {
            // from bower scripts
            var vendorBundle = new ScriptBundle(Bundles.Local.VendorScripts)
                // utils
                .IncludeDirectory("~/Content/vendor/scripts/utils/", "*.js")
                // jquery
                .Include(
                    "~/Content/vendor/scripts/jquery/jquery.js",
                    "~/Content/vendor/scripts/jquery/jquery-ui.js",
                    "~/Content/vendor/scripts//jquery-file-style.js",
                    "~/Content/vendor/scripts/jquery/jquery-file-download.js",
                    "~/Content/vendor/scripts/jquery/jquery-file-upload.js",
                    "~/Content/vendor/scripts/jquery/jquery-knob.js"
                )
                // bootstrap
                .Include(
                    "~/Content/vendor/scripts/bootstrap/bootstrap.js",
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
                    "~/Content/vendor/scripts/angular/angular-file-upload.js",
                    "~/Content/vendor/scripts/angular/angular-drag-and-drop-lists.js",
                    "~/Content/vendor/scripts/angular/sortable.js",
                    "~/Content/vendor/scripts/angular/clickoutside.directive.js"
                ).Include(
                    "~/Content/vendor/scripts/angular-ui-grid/ui-grid.js"
                )

                // angular
                .Include(
                    "~/Content/vendor/scripts/ace/ace.js",
                    "~/Content/vendor/scripts/ace/mode-xml.js",
                    "~/Content/vendor/scripts/ace/mode-csharp.js",
                    "~/Content/vendor/scripts/ace/ui-ace.js"
                )
                // dev extreme
                .Include(
                    "~/Content/vendor/scripts/devextreme/globalize.js",
                    "~/Content/vendor/scripts/devextreme/dx.chartjs.js",
                    "~/Content/vendor/scripts/devextreme/vectormap/usa.js"
                )
                // tiny-mce
                .Include(
                    "~/Content/vendor/scripts/tinymce/angular-ui-tinymce.js"
                )
                // pdf
                .Include(
                    "~/Content/vendor/scripts/pdf/pdf.combined.js",
                    "~/Content/vendor/scripts/pdf/angular-pdf.js"
                );

            vendorBundle.Orderer = new PassthroughBundleOrderer(); // enforcing import order
            bundles.Add(vendorBundle);

            // customized vendor scripts
            bundles.Add(new ScriptBundle(Bundles.Local.CustomVendorScripts)
                .IncludeDirectory("~/Content/customVendor/scripts/", "*.js", true)
                );

            // app scripts
            var appBundle = new ScriptBundle(Bundles.Local.AppScripts)
                //uncomment next line to test local html template caching
                //                .Include("~/Content/dist/scripts/htmltemplates.js")

                //                .Include("~/Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js")
                .Include("~/Content/Scripts/client/crud/aaa_layout.js")

//                .IncludeDirectory("~/Content/Shared/webcommons/scripts/softwrench/util", "*.js")
//                .IncludeDirectory("~/Content/Shared/webcommons", "*.js", true)
                .IncludeDirectory("~/Content/Scripts/client/crud", "*.js", true)
                .IncludeDirectory("~/Content/Scripts/client/services/", "*.js",true)
                .IncludeDirectory("~/Content/Scripts/client/controllers/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/constants/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/adminresources/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/components/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/util/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/directives/", "*.js",true)
                .IncludeDirectory("~/Content/Templates/commands", "*.js", true)
                .IncludeDirectory("~/Content/modules", "*.js", true);
            appBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(appBundle);
        }

        private void PopulateDistributionScriptBundles(BundleCollection bundles) {
            var scriptBundle = new ScriptBundle(Bundles.Distribution.AllScripts)
                .Include("~/Content/dist/scripts/vendor.js")
                .Include("~/Content/dist/scripts/customvendor.js")
                .Include("~/Content/dist/scripts/app.js")
                .Include("~/Content/dist/scripts/htmltemplates.js");
            scriptBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(scriptBundle);
        }

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }
    }
}