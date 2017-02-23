using System;
using System.Web.Optimization;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;
using softWrench.sW4.Web.Util.StaticFileLoad;

namespace softWrench.sW4.Web {
    public class BundleConfig {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles) {

            //            BundleTable.EnableOptimizations = true;

            AddScripts(bundles);


            AddStyles(bundles);
        }

        private static void AddStyles(BundleCollection bundles) {
            bundles.Add(new StyleBundle("~/Content/bootstrap/css/twitter-bootstrap").Include(
                "~/Content/bootstrap/css/bootstrap-min.css",
                "~/Content/bootstrap/css/bootstrap-theme-min.css",
                //                        "~/Content/bootstrap/css/bootstrap-responsive.css",
                "~/Content/bootstrap/css/bootstrap-combobox.css",
                "~/Content/bootstrap/css/datepicker.css",
                "~/Content/bootstrap/css/bootstrap-select.css",
                "~/Content/bootstrap/css/datetimepicker.css",
                "~/Content/bootstrap/css/submenu.css",
                "~/Content/bootstrap/css/typeahead.js-bootstrap.css",
                "~/Content/bootstrap/css/bootstrap-multiselect.css"
                ));
            AddClientBundle(bundles);


            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                "~/Content/themes/base/jquery.ui.core.css",
                "~/Content/themes/base/jquery.ui.resizable.css",
                "~/Content/themes/base/jquery.ui.selectable.css",
                "~/Content/themes/base/jquery.ui.accordion.css",
                "~/Content/themes/base/jquery.ui.autocomplete.css",
                "~/Content/themes/base/jquery.ui.button.css",
                "~/Content/themes/base/jquery.ui.dialog.css",
                "~/Content/themes/base/jquery.ui.slider.css",
                "~/Content/themes/base/jquery.ui.tabs.css",
                "~/Content/themes/base/jquery.ui.datepicker.css",
                "~/Content/themes/base/jquery.ui.progressbar.css",
                "~/Content/themes/base/jquery.ui.theme.css"));


            bundles.Add(new StyleBundle("~/Content/themes/base/font-awesome").Include(
                "~/Content/font-awesome-4.0.3/css/font-awesome.css"
                ));

            bundles.Add(new StyleBundle("~/Content/themes/base/fonts").Include(
                "~/Content/fonts/font.css"));
        }

        private static void PopulateDistributionScriptBundles(BundleCollection bundles) {
            var scriptBundle = new ScriptBundle("~/Content/dist/scripts")
                .Include("~/Content/dist/scripts/vendor.js")
                .Include("~/Content/dist/scripts/app.js");

//                            .Include("~/Content/temp/scripts/vendor.concat.js")
//                            .Include("~/Content/temp/scripts/app.concat.js");
            bundles.Add(scriptBundle);
        }

        private static void AddScripts(BundleCollection bundles) {
            if (!ApplicationConfiguration.IsLocal()) {
                PopulateDistributionScriptBundles(bundles);
                return;
            }

            PopulateLocalScriptBundles(bundles);
        }


        private static void PopulateLocalScriptBundles(BundleCollection bundles)
        {

            var vendorBundle = new ScriptBundle(Bundles.Local.VendorScripts)
                .Include(
                    "~/Content/Scripts/jquery/jquery-2.0.3-max.js",
                    "~/Content/Scripts/jquery/jquery-ui-1.10.3.js",
                    "~/Content/Scripts/jquery/jquery-file-style.js",
                    "~/Content/Scripts/jquery/jquery-filedownload-1.2.0.js",
                    "~/Content/Scripts/jquery/jquery-fileupload-5.40.1.js",
                    "~/Content/Scripts/spin-min.js"
                )

                .Include("~/Content/Scripts/thirdparty/*.js")
                .Include("~/Content/Scripts/jquery-ui-{version}.js")
                .Include("~/Content/Scripts/jquery.unobtrusive*", "~/Content/Scripts/jquery.validate*")
                .Include("~/Content/Scripts/thirdparty/jscrollpane/*.js")
//                .Include("~/Content/Scripts/modernizr-*")
                .Include("~/Content/Scripts/angular/angular.js",
                    "~/Content/Scripts/angular/angular-strap.js",
                    "~/Content/Scripts/angular/angular-sanitize.js",
                    "~/Content/Scripts/angular/bindonce.js"
                ).Include(
                    "~/Content/Scripts/bootstrap.max.js",
                    "~/Content/Scripts/bootstrap-datepicker.js",
                    "~/Content/Scripts/bootstrap-combobox.js",
                    "~/Content/Scripts/bootstrap-datetimepicker.js",
                    "~/Content/Scripts/bootstrap-collapse.js",
                    "~/Content/Scripts/bootbox.js",
                    "~/Content/Scripts/typeahead.js",
                    "~/Content/Scripts/hogan.js",
                    "~/Content/Scripts/locales/bootstrap-datepicker.de.js",
                    "~/Content/Scripts/locales/bootstrap-datetimepicker.de.js",
                    "~/Content/Scripts/locales/bootstrap-datepicker.es.js",
                    "~/Content/Scripts/locales/bootstrap-datetimepicker.es.js",
                    "~/Content/Scripts/modal.js",
                    "~/Content/Scripts/bootstrap-multiselect.js"
                ).Include("~/Content/Scripts/ace/ace.js").
                Include("~/Content/Scripts/angulartreeview.js");
            


            vendorBundle.Orderer = new PassthroughBundleOrderer(); // enforcing import order
            bundles.Add(vendorBundle);


            // app scripts
            var appBundle = new ScriptBundle(Bundles.Local.AppScripts)
                .Include(
                "~/Content/Scripts/signin.js",
                "~/Content/Scripts/client/*.js",
                "~/Content/Scripts/client/services/*.js",
                "~/Content/Scripts/client/directives/*.js",
                "~/Content/Scripts/client/components/*.js",
                "~/Content/Scripts/client/util/*.js"
                );


            var clientName = ApplicationConfiguration.ClientName;
            var clientPath = String.Format("~/Content/Scripts/customers/{0}", clientName);
            
            try {
                bundles.Add(appBundle.IncludeDirectory(clientPath, "*.js"));
            } catch {
                //nothing to do
            }
        
            appBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(appBundle);
        }

        private static void AddClientBundle(BundleCollection bundles) {
            var clientName = ApplicationConfiguration.ClientName;
            const string basePath = "~/Content/styles/default/";
            var clientPath = String.Format("~/Content/styles/{0}", clientName);
            var clientPathAppCustom = String.Format("~/Content/styles/{0}/application", clientName);
            var styleBundle = new StyleBundle("~/Content/styles/client/client-css");
            bundles.Add(styleBundle.IncludeDirectory(basePath, "*.css"));
            //client specific scripts go after, so they can override default styles
            try {
                bundles.Add(styleBundle.IncludeDirectory(clientPath, "*.css"));
                bundles.Add(styleBundle.IncludeDirectory(clientPathAppCustom, "*.css"));
            } catch {
                //nothing to do
            }
        }
    }
}