using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using log4net;
using SimpleInjector;
using softWrench.sW4.SimpleInjector.Core.Order;
using softWrench.sW4.Util;

namespace softWrench.sW4.SimpleInjector.Events {
    public class SimpleInjectorDomainEventDispatcher : IEventDispatcher {
        private readonly Container _container;

        private readonly IDictionary<Type, object> _cachedHandlers = new Dictionary<Type, object>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleInjectorDomainEventDispatcher));

        public SimpleInjectorDomainEventDispatcher(Container container) {
            _container = container;
        }


        public void Dispatch<T>(T eventToDispatch) where T : class {
            var handlers = FindHandlers<T>(eventToDispatch);
            
            foreach (var item in handlers) {
                var before = Stopwatch.StartNew();
                if (item is MultiThreadedSWEventListener<T>) {
                    var item1 = item;
                    var task = Task.Factory.NewThread(() => item1.HandleEvent(eventToDispatch));
                } else {
                    item.HandleEvent(eventToDispatch);
                }
                Log.Debug(LoggingUtil.BaseDurationMessage(item.GetType() + "runned in {0} ms", before));
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private List<ISWEventListener<T>> FindHandlers<T>(object eventToDispatch) where T : class {
            if (_cachedHandlers.ContainsKey(eventToDispatch.GetType())) {
                return (List<ISWEventListener<T>>)_cachedHandlers[eventToDispatch.GetType()];
            }
            var handlers = _container.GetAllInstances<ISWEventListener<T>>();
            var swEventListeners = new List<ISWEventListener<T>>((IEnumerable<ISWEventListener<T>>)handlers);
            _cachedHandlers.Add(eventToDispatch.GetType(), swEventListeners);
            OrderComparator<ISWEventListener<T>>.Sort(swEventListeners);
            return swEventListeners;
        }
    }
}