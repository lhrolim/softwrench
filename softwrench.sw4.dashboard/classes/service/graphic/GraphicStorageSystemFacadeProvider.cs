using System.Collections.Generic;
using System.Linq;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.dashboard.classes.service.graphic.exception;

namespace softwrench.sw4.dashboard.classes.service.graphic {
    public class GraphicStorageSystemFacadeProvider : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IDictionary<string, IGraphicStorageSystemFacade> _serviceRegistry = new Dictionary<string, IGraphicStorageSystemFacade>();

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var services = SimpleInjectorGenericFactory.Instance.GetObjectsOfType<IGraphicStorageSystemFacade>(typeof(IGraphicStorageSystemFacade));
            services.ToList().ForEach(s => _serviceRegistry[s.SystemName()] = s);
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

    }
}
