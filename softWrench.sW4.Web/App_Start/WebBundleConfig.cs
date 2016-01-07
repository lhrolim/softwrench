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

            bundles.Add(new StyleBundle("~/Content/fonts").Include(
                "~/Content/fonts/font.css"
                ));
        }

        public void PopulateScriptBundles(BundleCollection bundles) {

            // from bower
            bundles.Add(new ScriptBundle("~/Content/vendor/scripts").Include(
                "~/Content/vendor/scripts/*.js"
            ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/jquery/jquery").Include(
                 "~/Content/Scripts/vendor/jquery/jquery-2.0.3-max.js",
                 "~/Content/Scripts/vendor/jquery/jquery-ui-1.10.3.js",
                 "~/Content/Scripts/vendor/jquery/jquery-file-style.js",
                 "~/Content/Scripts/vendor/jquery/jquery-filedownload-1.2.0.js",
                 "~/Content/Scripts/vendor/jquery/zjquery-fileupload-5.40.1.js",
                 "~/Content/Scripts/vendor/other/spin-min.js"
                 ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/thirdparty").Include(
                "~/Content/Scripts/vendor/other/textAngular-sanitize.js",
                "~/Content/Scripts/vendor/other/textAngular-min.js",
                "~/Content/Scripts/vendor/other/textAngular-setup.js",
                "~/Content/Scripts/vendor/other/jquery.scannerdetection.js",
                "~/Content/Scripts/vendor/other/angular-fileUpload.js",
                "~/Content/Scripts/vendor/other/selectize/selectize.js",
                "~/Content/Scripts/vendor/other/selectize/angular-selectize.js",
                "~/Content/Scripts/vendor/other/angular-file-dnd.js",
                "~/Content/Scripts/vendor/other/lz-string.js")
                //"~/Content/Scripts/vendor/other/angular-ui-select.js")
                );

            bundles.Add(new ScriptBundle("~/Content/Scripts/jqueryui").Include(
                "~/Content/Scripts/vendor/jquery/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/jqueryval").Include(
                "~/Content/Scripts/vendor/jquery/jquery.unobtrusive*",
                "~/Content/Scripts/vendor/jquery/query.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/Content/Scripts/modernizr").Include(
                "~/Content/Scripts/vendor/other/modernizr-*"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/angular/angular").Include(
                "~/Content/Scripts/vendor/angular/angular.js",
                "~/Content/Scripts/vendor/angular/angular-strap.js",
                "~/Content/Scripts/vendor/angular/angular-sanitize.js",
                "~/Content/Scripts/vendor/angular/bindonce.js",
                "~/Content/Scripts/vendor/angular/components/*.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/twitter-bootstrap").Include(
                "~/Content/Scripts/vendor/other/moment.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap.max.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-datepicker.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-combobox.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-datetimepicker.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-collapse.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-richtext.js",
                "~/Content/Scripts/vendor/other/bootbox.js",
                "~/Content/Scripts/vendor/other/typeahead.js",
                "~/Content/Scripts/vendor/moment/locales/de.js",
                "~/Content/Scripts/vendor/moment/locales/es.js",
                "~/Content/Scripts/vendor/bootstrap/modal.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/ace/ace").Include(
                "~/Content/Scripts/vendor/ace/ace.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin").Include(
                "~/Content/Scripts/client/signin/signin.js"
                ));

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

            bundles.Add(new ScriptBundle("~/Content/Scripts/thirdparty/graphics").Include(
                "~/Content/Scripts/vendor/other/tableau/tableau-2.0.0-min.js"));
        }

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }
    }
}