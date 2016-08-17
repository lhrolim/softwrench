using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
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

        public void Dispatch<T>(T eventToDispatch, bool parallel = false) where T : class {
            var handlers = FindHandlers<T>(eventToDispatch);
            var eventName = eventToDispatch.GetType().Name;
            var shouldRunInParallel = handlers.Count > 1 && parallel && AllowsParallelDispatch(eventToDispatch);

            var before = Stopwatch.StartNew();
            // parallel: up to .NET to decide how many threads to use
            if (shouldRunInParallel) {
                Log.DebugFormat("Running parallel dispatch for event {0}", eventName);
                Parallel.ForEach(handlers, item => RunEventHandler(item, eventToDispatch));
                Log.DebugFormat("Parallel dispatch for {0} finished in {1} ms", eventName, LoggingUtil.MsDelta(before));
                return;
            }
            // sequential execution
            Log.DebugFormat("Running sequential iteration dispatch for event {0}", eventName);
            foreach (var item in handlers) {
                RunEventHandler(item, eventToDispatch);
            }
            Log.DebugFormat("Finished sequential iteration dispatch for event {0} in {1} ms", eventName, LoggingUtil.MsDelta(before));
        }

        public void DispatchAsync<T>(T eventToDispatch, bool parallel = false) where T : class {
            Log.DebugFormat("Running Fire-and-forget dispatch for event {0}", eventToDispatch.GetType().Name);
            Task.Run(() => Dispatch(eventToDispatch, parallel)).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private List<ISWEventListener<T>> FindHandlers<T>(object eventToDispatch) where T : class {
            if (_cachedHandlers.ContainsKey(eventToDispatch.GetType())) {
                return (List<ISWEventListener<T>>)_cachedHandlers[eventToDispatch.GetType()];
            }
            var handlers = _container.GetAllInstances<ISWEventListener<T>>();
            var swEventListeners = new List<ISWEventListener<T>>((IEnumerable<ISWEventListener<T>>) handlers);
            _cachedHandlers.Add(eventToDispatch.GetType(), swEventListeners);
            OrderComparator<ISWEventListener<T>>.Sort(swEventListeners);
            return swEventListeners;
        }
    }
}