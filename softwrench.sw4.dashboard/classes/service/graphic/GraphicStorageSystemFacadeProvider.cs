using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using log4net;
using softwrench.sw4.dashboard.classes.service.graphic.exception;

namespace softwrench.sw4.dashboard.classes.service.graphic {
    /// <summary>
    /// Factory for <see cref="IGraphicStorageSystemFacade"/>. 
    /// During application bootstrap resolves all <see cref="IGraphicStorageSystemFacade"/> and caches them (indexed by their SystemName).
    /// In order for this to work as expected the SystemName has to be unique among the services: 
    /// there's no guarantee as to which service instance will be the Facade of a particullar system if more than one share the same SystemName. 
    /// </summary>
    public class GraphicStorageSystemFacadeProvider : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(GraphicStorageSystemFacadeProvider));

        private readonly IDictionary<string, IGraphicStorageSystemFacade> _serviceRegistry = new ConcurrentDictionary<string, IGraphicStorageSystemFacade>();

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            SimpleInjectorGenericFactory.Instance
                .GetObjectsOfType<IGraphicStorageSystemFacade>(typeof(IGraphicStorageSystemFacade))
                .ToList()
                .ForEach(service => _serviceRegistry[service.SystemName] = service);
        }

        /// <summary>
        /// Finds the instance of the service that supports/connects to the graphic storage system with name systemName.
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public IGraphicStorageSystemFacade GetService(string systemName) {
            IGraphicStorageSystemFacade service;
            if (_serviceRegistry.TryGetValue(systemName, out service)) {
                return service;
            }
            throw GraphicStorageSystemException.ServiceNotFound(systemName);
        }

        /// <summary>
        /// Manually register a <see cref="IGraphicStorageSystemFacade"/> instance.
        /// If there's a service already registered for the same service.SystemName the instance will be replaced. 
        /// </summary>
        /// <param name="service"></param>
        public void RegisterService(IGraphicStorageSystemFacade service) {
            var systemName = service.SystemName;
            if (_serviceRegistry.ContainsKey(systemName)) {
                Log.WarnFormat("Overriding registered IGraphicStorageSystemFacade instance for graphic system '{0}'", systemName);
            }
            _serviceRegistry[systemName] = service;
        }

    }
}
