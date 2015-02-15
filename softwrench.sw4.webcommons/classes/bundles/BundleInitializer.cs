using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;
using cts.commons.simpleinjector.Events;
using cts.commons.web;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.webcommons.classes.bundles {
    public class BundleInitializer : ISWEventListener<ApplicationStartedEvent>, ISWEventListener<ClientChangeEvent> {

        private readonly IList<IBundleConfigProvider> _bundleConfigs;

        public BundleInitializer(IList<IBundleConfigProvider> bundleConfigs) {
            _bundleConfigs = bundleConfigs;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            InitBundles();
        }

        public void HandleEvent(ClientChangeEvent eventToDispatch) {
            InitBundles();
        }

        private void InitBundles() {
            BundleTable.Bundles.Clear();
            foreach (var bundleConfigProvider in _bundleConfigs) {
                bundleConfigProvider.PopulateScriptBundles(BundleTable.Bundles);
                bundleConfigProvider.PopulateStyleBundles(BundleTable.Bundles);
            }
        }


    }
}
