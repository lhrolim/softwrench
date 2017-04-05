using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.api.classes.application;

namespace softwrench.sw4.offlineserver.services {
    public class SynchronizationManagerProvider : ApplicationFiltereableProvider<SynchronizationManager> {

        private readonly SynchronizationManager _defaultInstance;

        public SynchronizationManagerProvider(SynchronizationManager defaultInstance) {
            _defaultInstance = defaultInstance;
        }


        protected override SynchronizationManager LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return _defaultInstance;
        }
    }
}
