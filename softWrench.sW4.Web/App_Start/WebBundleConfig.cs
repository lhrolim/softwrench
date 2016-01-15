﻿using System.Web.Optimization;
using cts.commons.web;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util.StaticFileLoad;

namespace softWrench.sW4.Web {
    public class WebBundleConfig : IBundleConfigProvider {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725

        public void PopulateStyleBundles(BundleCollection bundles) {
            if (ApplicationConfiguration.IsLocal()) {
                PopulateLocalStyleBundles(bundles);
            } else {
                PopulateDistributionStyleBundles(bundles);
            }

            // from bower fonts
            bundles.Add(new StyleBundle(Bundles.Local.FontsStyles)
                .Include("~/Content/fonts/font.css")
                );
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
                .Include("~/Content/vendor/css/angular/textAngular.css");
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
            if (ApplicationConfiguration.IsLocal()) {
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
                    "~/Content/vendor/scripts/jquery/jquery-file-upload.js"
                )
                // bootstrap
                .Include(
                    "~/Content/vendor/scripts/bootstrap/bootstrap.js",
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
            bundles.Add(new ScriptBundle(Bundles.Local.CustomVendorScripts)
                .IncludeDirectory("~/Content/customVendor/scripts/", "*.js", true)
                );

            // app scripts
            var appBundle = new ScriptBundle(Bundles.Local.AppScripts)
                .Include("~/Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js")
                .Include("~/Content/Scripts/client/crud/aaa_layout.js")
                .IncludeDirectory("~/Content/Shared/webcommons/scripts/softwrench/util", "*.js")
                .IncludeDirectory("~/Content/Shared/webcommons", "*.js", true)
                .IncludeDirectory("~/Content/Scripts/client/crud", "*.js", true)
                .IncludeDirectory("~/Content/Scripts/client/services/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/adminresources/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/directives/", "*.js")
                .IncludeDirectory("~/Content/Scripts/client/directives/menu/", "*.js")
                .IncludeDirectory("~/Content/Templates/commands", "*.js", true)
                .IncludeDirectory("~/Content/modules", "*.js", true);
            appBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(appBundle);
        }

        private void PopulateDistributionScriptBundles(BundleCollection bundles) {
            var scriptBundle = new ScriptBundle(Bundles.Distribution.AllScripts)
                .Include("~/Content/dist/scripts/vendor.js")
                .Include("~/Content/dist/scripts/app.js");
            scriptBundle.Orderer = new PassthroughBundleOrderer();
            bundles.Add(scriptBundle);
        }

        public static void ClearBundles() {
            BundleTable.Bundles.Clear();
        }
    }
}