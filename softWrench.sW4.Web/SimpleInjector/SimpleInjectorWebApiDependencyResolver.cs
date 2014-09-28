using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http.Dependencies;
using SimpleInjector;

namespace softWrench.sW4.Web.SimpleInjector {
    public sealed class SimpleInjectorWebApiDependencyResolver : IDependencyResolver {
        private readonly Container _container;

        public SimpleInjectorWebApiDependencyResolver(
            Container container) {
            _container = container;
        }

        [DebuggerStepThrough]
        public IDependencyScope BeginScope() {
            return this;
        }

        [DebuggerStepThrough]
        public object GetService(Type serviceType) {
            return ((IServiceProvider)_container).GetService(serviceType);
        }

        [DebuggerStepThrough]
        public IEnumerable<object> GetServices(Type serviceType) {
            return _container.GetAllInstances(serviceType);
        }

        [DebuggerStepThrough]
        public void Dispose() {
        }
    }
}