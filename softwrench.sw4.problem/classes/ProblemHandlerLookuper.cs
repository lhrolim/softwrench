using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using SimpleInjector;

namespace softwrench.sw4.problem.classes {

    public class ProblemHandlerLookuper : ISingletonComponent {

        private readonly IDictionary<ProblemHandlerKey, IProblemHandler> _cachedHandlers = new ConcurrentDictionary<ProblemHandlerKey, IProblemHandler>();

        private readonly Container _container;
        private readonly IApplicationConfiguration _appConfig;

        public ProblemHandlerLookuper(Container container, IApplicationConfiguration appConfig) {
            _container = container;
            _appConfig = appConfig;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public IProblemHandler FindHandler(string handlerName, string applicationName) {
            if (handlerName == null) {
                return null;
            }

            var clientKey = _appConfig.GetClientKey();

            var handlerKey = new ProblemHandlerKey(clientKey, handlerName, applicationName);
            if (_cachedHandlers.ContainsKey(handlerKey)) {
                return _cachedHandlers[handlerKey];
            }
            var handlers = _container.GetAllInstances<IProblemHandler>();
            if (handlers == null) {
                return null;
            }
            //TODO:improve lookup logic to allow for non-perfect matches
            var handler = handlers.FirstOrDefault(f => f.ApplicationName().EqualsIc(applicationName) && f.ProblemHandler().EqualsIc(handlerName) && f.ClientName().EqualsIc(clientKey));
            _cachedHandlers.Add(handlerKey, handler);
            return handler;
        }

        class ProblemHandlerKey {
            public ProblemHandlerKey(string clientName, string handlerName, string applicationName) {
                ClientName = clientName;
                HandlerName = handlerName;
                ApplicationName = applicationName;
            }

            string ClientName {
                get; set;
            }
            private string HandlerName {
                get; set;
            }
            private string ApplicationName {
                get; set;
            }

            protected bool Equals(ProblemHandlerKey other) {
                return string.Equals(HandlerName, other.HandlerName)
                    && string.Equals(ClientName, other.ClientName)
                    && string.Equals(ApplicationName, other.ApplicationName);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ProblemHandlerKey)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (ClientName != null ? ClientName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (HandlerName != null ? HandlerName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (ApplicationName != null ? ApplicationName.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }


    }
}
