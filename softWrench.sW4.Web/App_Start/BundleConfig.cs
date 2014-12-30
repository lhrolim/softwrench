using System;
using System.Web.Optimization;
using softWrench.sW4.Util;

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
                "~/Content/bootstrap/css/bootstrap-multiselect.css",
                "~/Content/bootstrap/css/textAngular-min.css"
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
                "~/Content/font-awesome-4.1.0/css/font-awesome.css"
                ));

            bundles.Add(new StyleBundle("~/Content/themes/base/fonts").Include(
                "~/Content/fonts/font.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/ie9").Include(
                "~/Content/ie/ie9.css"
                ));
        }

        private static void AddScripts(BundleCollection bundles) {
            // When try
            bundles.Add(new ScriptBundle("~/Content/Scripts/jquery/jquery").Include(
                "~/Content/Scripts/jquery/jquery-2.0.3-max.js",
                "~/Content/Scripts/jquery/jquery-ui-1.10.3.js",
                "~/Content/Scripts/jquery/jquery-file-style.js",
                "~/Content/Scripts/jquery/jquery-filedownload-1.2.0.js",
                "~/Content/Scripts/jquery/jquery-fileupload-5.40.1.js",
                "~/Content/Scripts/spin-min.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/thirdparty").Include(
                "~/Content/Scripts/thirdparty/moment.js",
                "~/Content/Scripts/thirdparty/textAngular-sanitize-min.js",
                "~/Content/Scripts/thirdparty/textAngular-min.js",
                "~/Content/Scripts/thirdparty/jquery.scannerdetection.js",
                "~/Content/Scripts/thirdparty/angular-fileUpload.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/jqueryui").Include(
                "~/Content/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/jqueryval").Include(
                "~/Content/Scripts/jquery.unobtrusive*",
                "~/Content/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/Content/Scripts/modernizr").Include(
                "~/Content/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/angular/angular").Include(
                "~/Content/Scripts/" + "angular/angular.js",
                "~/Content/Scripts/" + "angular/angular-strap.js",
                "~/Content/Scripts/" + "angular/angular-sanitize.js",
                "~/Content/Scripts/" + "angular/bindonce.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/twitter-bootstrap").Include(
                "~/Content/Scripts/bootstrap.max.js",
                "~/Content/Scripts/bootstrap-datepicker.js",
                "~/Content/Scripts/bootstrap-combobox.js",
                "~/Content/Scripts/bootstrap-datetimepicker.js",
                "~/Content/Scripts/bootstrap-collapse.js",
                "~/Content/Scripts/bootstrap-richtext.js",
                "~/Content/Scripts/bootbox.js",
                "~/Content/Scripts/typeahead.js",
                "~/Content/Scripts/hogan.js",
                "~/Content/Scripts/locales/bootstrap-datepicker.de.js",
                "~/Content/Scripts/locales/bootstrap-datetimepicker.de.js",
                "~/Content/Scripts/locales/bootstrap-datepicker.es.js",
                "~/Content/Scripts/locales/bootstrap-datetimepicker.es.js",
                "~/Content/Scripts/modal.js",
                "~/Content/Scripts/bootstrap-multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/ace/ace").Include(
                "~/Content/Scripts/ace/ace.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin").Include(
                "~/Content/Scripts/signin.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application").Include(
                "~/Content/Scripts/client/*.js",
                "~/Content/Scripts/client/services/*.js",
                "~/Content/Scripts/client/directives/*.js",
                "~/Content/Scripts/client/components/*.js",
                "~/Content/Scripts/client/util/*.js"
                ).IncludeDirectory("~/Content/Templates/commands","*.js",true));

            var clientName = ApplicationConfiguration.ClientName;
            var clientPath = String.Format("~/Content/Scripts/customers/{0}", clientName);
            const string sharedPath = "~/Content/Scripts/customers/shared";
            var scriptBundle = new ScriptBundle("~/Content/Scripts/client/client-js");
            bundles.Add(scriptBundle.IncludeDirectory(sharedPath, "*.js"));
            try {
                bundles.Add(scriptBundle.IncludeDirectory(clientPath, "*.js"));
            }
            catch {
                //nothing to do
            }
        }

        private static void AddClientBundle(BundleCollection bundles) {
            var clientName = ApplicationConfiguration.ClientName;
            const string basePath = "~/Content/styles/default/";
            const string baseAppPath = basePath + "/application";
            const string baseMediaPath = basePath + "/media";

            var clientPath = String.Format("~/Content/styles/{0}", clientName);
            var clientPathAppCustom = clientPath + "/application";
            var clientPathMediaCustom = clientPath + "/media";

            var styleBundle = new StyleBundle("~/Content/styles/client/client-css");
            
            bundles.Add(styleBundle.IncludeDirectory(basePath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseAppPath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseMediaPath, "*.css"));

            //client specific scripts go after, so they can override default styles
            try {
                bundles.Add(styleBundle.IncludeDirectory(clientPath, "*.css"));
            }
            catch {
                //nothing to do
            }
            try
            {
                bundles.Add(styleBundle.IncludeDirectory(clientPathAppCustom, "*.css"));
            }
            catch
            {
                //nothing to do
            }
            try
            {
                bundles.Add(styleBundle.IncludeDirectory(clientPathMediaCustom, "*.css"));
            }
            catch
            {
                //nothing to do
            }
        }
    }
}