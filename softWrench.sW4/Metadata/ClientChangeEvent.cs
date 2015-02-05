using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.ExtendedProperties;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata {
    public class ClientChangeEvent : ISWEvent {

        private readonly string _clientKey;
        private readonly Boolean _restore;

        public ClientChangeEvent(string clientKey, bool restore) {
            if (!(ApplicationConfiguration.IsDev() || !ApplicationConfiguration.IsLocal())) {
                throw new InvalidOperationException("this event can only be run in development mode");
            }
            _clientKey = clientKey;
            _restore = restore;
            if (!restore && clientKey == null) {
                throw new InvalidOperationException("clientkey cannot be null");
            }
        }

        public string ClientKey {
            get { return _clientKey; }
        }

        public bool Restore {
            get { return _restore; }
        }

        public bool AllowMultiThreading { get { return false; } }
    }
}
