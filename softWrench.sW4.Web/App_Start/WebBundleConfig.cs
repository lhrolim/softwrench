using System;
using System.Web.Optimization;
using cts.commons.web;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web {
    public class WebBundleConfig :IBundleConfigProvider{
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725

        public void PopulateStyleBundles(BundleCollection bundles) {
            bundles.Add(new StyleBundle("~/Content/bootstrap/css/twitter-bootstrap").Include(
               "~/Content/bootstrap/css/bootstrap-min.css",
               "~/Content/bootstrap/css/bootstrap-theme-min.css",
               "~/Content/bootstrap/css/bootstrap-combobox.css",
               "~/Content/bootstrap/css/datepicker.css",
               "~/Content/bootstrap/css/bootstrap-select.css",
               "~/Content/bootstrap/css/datetimepicker.css",
               "~/Content/bootstrap/css/submenu.css",
               "~/Content/bootstrap/css/typeahead.js-bootstrap.css",
               "~/Content/bootstrap/css/bootstrap-multiselect.css",
               "~/Content/bootstrap/css/textAngular-min.css"
               ));
//            AddClientBundle(bundles);


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

        public void PopulateScriptBundles(BundleCollection bundles) {
            bundles.Add(new ScriptBundle("~/Content/Scripts/jquery/jquery").Include(
                 "~/Content/Scripts/vendor/jquery/jquery-2.0.3-max.js",
                 "~/Content/Scripts/vendor/jquery/jquery-ui-1.10.3.js",
                 "~/Content/Scripts/vendor/jquery/jquery-file-style.js",
                 "~/Content/Scripts/vendor/jquery/jquery-filedownload-1.2.0.js",
                 "~/Content/Scripts/vendor/jquery/zjquery-fileupload-5.40.1.js",
                 "~/Content/Scripts/vendor/other/spin-min.js"
                 ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/thirdparty").Include(
                "~/Content/Scripts/vendor/other/moment.js",
                "~/Content/Scripts/vendor/other/textAngular-sanitize.js",
                "~/Content/Scripts/vendor/other/textAngular-min.js",
                "~/Content/Scripts/vendor/other/textAngular-setup.js",
                "~/Content/Scripts/vendor/other/jquery.scannerdetection.js",
                "~/Content/Scripts/vendor/other/angular-fileUpload.js"));

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
                "~/Content/Scripts/vendor/angular/angular-strap.js",
                "~/Content/Scripts/vendor/angular/angular-sanitize.js",
                "~/Content/Scripts/vendor/angular/bindonce.js",
                "~/Content/Scripts/vendor/angular/components/*.js"

                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/twitter-bootstrap").Include(
                "~/Content/Scripts/vendor/bootstrap/bootstrap.max.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-datepicker.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-combobox.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-datetimepicker.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-collapse.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-richtext.js",
                "~/Content/Scripts/vendor/other/bootbox.js",
                "~/Content/Scripts/vendor/other/typeahead.js",
                "~/Content/Scripts/vendor/bootstrap/locales/bootstrap-datepicker.de.js",
                "~/Content/Scripts/vendor/bootstrap/locales/bootstrap-datetimepicker.de.js",
                "~/Content/Scripts/vendor/bootstrap/locales/bootstrap-datepicker.es.js",
                "~/Content/Scripts/vendor/bootstrap/locales/bootstrap-datetimepicker.es.js",
                "~/Content/Scripts/vendor/bootstrap/modal.js",
                "~/Content/Scripts/vendor/bootstrap/bootstrap-multiselect.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/ace/ace").Include(
                "~/Content/Scripts/vendor/ace/ace.js"));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/signin").Include(
                "~/Content/Scripts/client/signin.js"
                ));

            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application").Include(
                "~/Content/Scripts/client/crud/*.js",
                "~/Content/Scripts/client/services/*.js",
                "~/Content/Scripts/client/*.js",
                "~/Content/Scripts/client/adminresources/*.js",
                "~/Content/Scripts/client/directives/*.js",
                "~/Content/Scripts/client/directives/menu/*.js"
                ).IncludeDirectory("~/Content/Templates/commands", "*.js", true));

        
        }
        
        

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }

      

       
    }
}