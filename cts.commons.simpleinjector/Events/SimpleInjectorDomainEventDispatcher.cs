using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.Util;
using log4net;
using SimpleInjector;

namespace cts.commons.simpleinjector.Events {
    public class SimpleInjectorDomainEventDispatcher : IEventDispatcher {
        private readonly Container _container;

        private readonly IDictionary<Type, object> _cachedHandlers = new Dictionary<Type, object>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(SimpleInjectorDomainEventDispatcher));

        public SimpleInjectorDomainEventDispatcher(Container container) {
            _container = container;
        }

        private void RunEventHandler<T>(ISWEventListener<T> handler, T dispatchedEvent) {
            var before = Stopwatch.StartNew();
            handler.HandleEvent(dispatchedEvent);
            Log.Debug(LoggingUtil.BaseDurationMessage(handler.GetType() + "ran in {0} ms", before));
        }

        /// <summary>
        /// Disallow only subtypes of ISWEvent that have AllowMultiThreading == false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dispatchedEvent"></param>
        /// <returns></returns>
        private bool AllowsParallelDispatch<T>(T dispatchedEvent) {
            return !dispatchedEvent.GetType().IsSubclassOf(typeof (ISWEvent)) || ((ISWEvent) dispatchedEvent).AllowMultiThreading;
        }

        /// <summary>
        /// Dispatches the events to it's handlers:
        /// - first: executes <see cref="IPriorityOrdered"/> handlers sequentially in order
        /// - second: executes non-ordered handlers sequentially or in parallel
        /// - last: executes <see cref="IOrdered"/> handlers sequentially in order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventToDispatch"></param>
        /// <param name="parallel"></param>
        public void Dispatch<T>(T eventToDispatch, bool parallel = false) where T : class {
            var handlers = FindHandlers<T>(eventToDispatch);
            var eventName = eventToDispatch.GetType().Name;

            // priority handlers
            Log.DebugFormat("Dispatching event {0} to priority handlers", eventName);
            DispatchToHandlerGroup(handlers.PriorityHandlers, eventToDispatch);

            // non-ordered handlers
            var shouldRunNonOrderedInParallel = handlers.NonOrderedHandlers.Count > 1 && parallel && AllowsParallelDispatch(eventToDispatch);
            Log.DebugFormat("Dispatching event {0} to non-ordered handlers", eventName);
            DispatchToHandlerGroup(handlers.NonOrderedHandlers, eventToDispatch, shouldRunNonOrderedInParallel);

            // ordered handlers
            Log.DebugFormat("Dispatching event {0} to ordered handlers", eventName);
            DispatchToHandlerGroup(handlers.OrderedHandlers, eventToDispatch);
        }

        /// <summary>
        /// Dispatches the event (<see cref="Dispatch{T}"/>) in a fire-and-forget manner.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventToDispatch"></param>
        /// <param name="parallel"></param>
        public void Fire<T>(T eventToDispatch, bool parallel = false) where T : class {
            Log.DebugFormat("Running Fire-and-forget dispatch for event {0}", eventToDispatch.GetType().Name);
            Task.Run(() => Dispatch(eventToDispatch, parallel)).ConfigureAwait(false);
        }

        private void DispatchToHandlerGroup<T>(IList<ISWEventListener<T>> handlers, T eventToDispatch, bool parallel = false) {
            var eventName = eventToDispatch.GetType().Name;
            var before = Stopwatch.StartNew();
            // parallel: up to .NET to decide how many threads to use
            if (parallel) {
                Log.DebugFormat("Running parallel dispatch for event {0}", eventName);
                Parallel.ForEach(handlers, item => RunEventHandler(item, eventToDispatch));
                Log.DebugFormat("Parallel dispatch for {0} finished in {1} ms", eventName, LoggingUtil.MsDelta(before));
            }
            // sequential execution
            Log.DebugFormat("Running sequential iteration dispatch for event {0}", eventName);
            foreach (var handler in handlers) {
                RunEventHandler(handler, eventToDispatch);
            }
            Log.DebugFormat("Finished sequential iteration dispatch for event {0} in {1} ms", eventName, LoggingUtil.MsDelta(before));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private EventHandlerCollection<T> FindHandlers<T>(object eventToDispatch) where T : class {
            if (_cachedHandlers.ContainsKey(eventToDispatch.GetType())) {
                return (EventHandlerCollection<T>)_cachedHandlers[eventToDispatch.GetType()];
            }
            var handlers = _container.GetAllInstances<ISWEventListener<T>>();
            var handlerCollection = new EventHandlerCollection<T>(handlers);
            _cachedHandlers.Add(eventToDispatch.GetType(), handlerCollection);
            return handlerCollection;
        }

        /// <summary>
        /// Groups <see cref="ISWEventListener{T}"/> by priority.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class EventHandlerCollection<T> where T : class {
            /// <summary>
            /// Ordered list of handlers that are also <see cref="IPriorityOrdered"/>
            /// </summary>
            public List<ISWEventListener<T>> PriorityHandlers { get; private set; }
            /// <summary>
            /// Ordered list of handlers that are also <see cref="IOrdered"/>
            /// </summary>
            public List<ISWEventListener<T>> OrderedHandlers { get; private set; }
            /// <summary>
            /// Unordered list of handlers
            /// </summary>
            public List<ISWEventListener<T>> NonOrderedHandlers { get; private set; }

            public EventHandlerCollection(IEnumerable<ISWEventListener<T>> handlers) {
                PriorityHandlers = new List<ISWEventListener<T>>();
                OrderedHandlers = new List<ISWEventListener<T>>();
                NonOrderedHandlers = new List<ISWEventListener<T>>();

                foreach (var handler in handlers) {
                    if (handler is IPriorityOrdered) {
                        PriorityHandlers.Add(handler);
                        continue;
                    }
                    if (handler is IOrdered) {
                        OrderedHandlers.Add(handler);
                        continue;
                    }
                    NonOrderedHandlers.Add(handler);
                }

                OrderComparator<ISWEventListener<T>>.Sort(PriorityHandlers);
                OrderComparator<ISWEventListener<T>>.Sort(OrderedHandlers);
            }
        }
    }
}