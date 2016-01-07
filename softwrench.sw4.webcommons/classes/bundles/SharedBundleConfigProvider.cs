using System;
using System.Web.Optimization;
using cts.commons.web;
using softWrench.sW4.Util;

namespace softwrench.sw4.webcommons.classes.bundles {

    public class SharedBundleConfigProvider : IBundleConfigProvider {


        public void PopulateStyleBundles(BundleCollection bundles) {
            AddClientStyleBundle(bundles);
        }

        public void PopulateScriptBundles(BundleCollection bundles) {
            bundles.Add(new ScriptBundle("~/Content/Scripts/client/application/shared")
                .IncludeDirectory("~/Content/Shared/","*.js",true));

            var clientName = ApplicationConfiguration.ClientName;
            var clientPath = String.Format("~/Content/customers/{0}/Scripts/", clientName);
            //scritps specific of the online
            var clientOnlinePath = String.Format("~/Content/customers/{0}_online/Scripts/", clientName);
            const string sharedPath = "~/Content/Scripts/customers/shared";
            var scriptBundle = new ScriptBundle("~/Content/Scripts/client/client-js");
            bundles.Add(scriptBundle.IncludeDirectory(sharedPath, "*.js"));
            bundles.IgnoreList.Ignore("*.mobile.js");
            try {
                // Wanted OTB to load as the base template and then additional js can be applied to overwrite the existing one
                bundles.Add(scriptBundle.IncludeDirectory("~/Content/Scripts/customers/otb", "*.js"));
                bundles.Add(scriptBundle.IncludeDirectory(clientPath, "*.js"));
                bundles.Add(scriptBundle.IncludeDirectory(clientOnlinePath, "*.js"));
            } catch {
                //nothing to do
            }
        }

        private static void AddClientStyleBundle(BundleCollection bundles) {
            var clientName = ApplicationConfiguration.ClientName;
            const string basePath = "~/Content/styles/default/";
            const string baseAppPath = basePath + "/application";
            const string baseMediaPath = basePath + "/media";
            const string baseVendorPath = basePath + "/vendor";

            var clientPath = String.Format("~/Content/styles/{0}", clientName); ;
            if (AssemblyLocator.CustomerAssemblyExists()) {
                clientPath = String.Format("~/Content/customers/{0}/styles/", clientName);
            }

            var clientPathAppCustom = clientPath + "/application";
            var clientPathMediaCustom = clientPath + "/media";
            var clientPathVendorCustom = clientPath + "/vendor";

            var styleBundle = new StyleBundle("~/Content/styles/client/client-css");

            bundles.Add(new StyleBundle("~/Content/styles/shared").IncludeDirectory("~/Content/Shared/", "*.css", true));

            bundles.Add(styleBundle.IncludeDirectory(basePath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseAppPath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseMediaPath, "*.css"));
            bundles.Add(styleBundle.IncludeDirectory(baseVendorPath, "*.css"));

            bundles.IgnoreList.Ignore("*.mobile.css");

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
            try
            {
                bundles.Add(styleBundle.IncludeDirectory(clientPathVendorCustom, "*.css"));
            }
            catch
            {
                //nothing to do
            }
        }
    }
}
