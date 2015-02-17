using System;
using System.Web.Optimization;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using cts.commons.web;
using softWrench.sW4.Util;

namespace softwrench.sw4.webcommons.classes.bundles {

    public class SharedBundleConfigProvider : IBundleConfigProvider {


        public void PopulateStyleBundles(BundleCollection bundles) {
            AddClientBundle(bundles);
        }

        public void PopulateScriptBundles(BundleCollection bundles) {
            // When try
//         bundles.Add(new ScriptBundle("~/Content/Scripts/angular/angular").Include(
//                "~/Content/Scripts/vendor/angular/angular.js",
//                "~/Content/Scripts/vendor/angular/angular-strap.js",
//                "~/Content/Scripts/vendor/angular/angular-sanitize.js",
//                "~/Content/Scripts/vendor/angular/bindonce.js",
//                "~/Content/Scripts/vendor/angular/components/*.js"
//                ));


            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application/shared").Include(
                "~/Content/Scripts/softwrench/shared/components/*.js",
                "~/Content/Scripts/softwrench/shared/services/*.js",
                "~/Content/Scripts/softwrench/shared/util/*.js"
                ));

            var clientName = ApplicationConfiguration.ClientName;
            var clientPath = String.Format("~/Content/customers/{0}/Scripts/", clientName);
            const string sharedPath = "~/Content/Scripts/customers/shared";
            var scriptBundle = new ScriptBundle("~/Content/Scripts/client/client-js");
            bundles.Add(scriptBundle.IncludeDirectory(sharedPath, "*.js"));
            try {
                // Wanted OTB to load as the base template and then additional js can be applied to overwrite the existing one
                bundles.Add(scriptBundle.IncludeDirectory("~/Content/Scripts/customers/otb", "*.js"));
                bundles.Add(scriptBundle.IncludeDirectory(clientPath, "*.js"));
            } catch {
                //nothing to do
            }
        }

        private static void AddClientBundle(BundleCollection bundles) {
            var clientName = ApplicationConfiguration.ClientName;
            const string basePath = "~/Content/styles/default/";
            const string baseAppPath = basePath + "/application";
            const string baseMediaPath = basePath + "/media";

            var clientPath = String.Format("~/Content/customers/{0}/styles/", clientName);
            var clientPathAppCustom = clientPath + "/application";
            var clientPathMediaCustom = clientPath + "/media";

            var styleBundle = new StyleBundle("~/Content/styles/client/client-css");

            bundles.Add(styleBundle.IncludeDirectory(basePath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseAppPath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseMediaPath, "*.css"));

            //client specific scripts go after, so they can override default styles
            try {
                bundles.Add(styleBundle.IncludeDirectory(clientPath, "*.css"));
            } catch {
                //nothing to do
            }
            try {
                bundles.Add(styleBundle.IncludeDirectory(clientPathAppCustom, "*.css"));
            } catch {
                //nothing to do
            }
            try {
                bundles.Add(styleBundle.IncludeDirectory(clientPathMediaCustom, "*.css"));
            } catch {
                //nothing to do
            }
        }




    }
}
