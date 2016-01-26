using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;
using cts.commons.simpleinjector.Events;
using cts.commons.web;
using SimpleInjector;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.webcommons.classes.bundles {
    public class BundleInitializer : ISWEventListener<ApplicationStartedEvent>, ISWEventListener<ClientChangeEvent> {

        private readonly Container _container;

        public BundleInitializer(Container container) {
            _container = container;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            InitBundles();
        }

        public void HandleEvent(ClientChangeEvent eventToDispatch) {
            InitBundles();
        }

        private void InitBundles() {
            BundleTable.Bundles.Clear();
            var providers =_container.GetAllInstances<IBundleConfigProvider>();
            foreach (var bundleConfigProvider in providers) {
                bundleConfigProvider.PopulateScriptBundles(BundleTable.Bundles);
                bundleConfigProvider.PopulateStyleBundles(BundleTable.Bundles);
            }
        }


    }
}
