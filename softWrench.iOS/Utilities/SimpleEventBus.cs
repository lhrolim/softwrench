using System;
using System.Collections.Generic;

namespace softWrench.iOS.Utilities
{
    public class SimpleEventBus
    {
        private readonly static IDictionary<Type, IList<object>> subscribers = new Dictionary<Type, IList<object>>();
        private readonly static object monitor = new object();

        public static void Subscribe<T>(Action<T> handler)
        {
            IList<object> subscribersOfT;

            if (false == subscribers.TryGetValue(typeof(T), out subscribersOfT))
            {
                lock (monitor)
                {
                    if (false == subscribers.TryGetValue(typeof(T), out subscribersOfT))
                    {
                        subscribersOfT = new List<object>();
                        subscribers.Add(typeof(T), subscribersOfT);
                    }
                }
            }

            if (subscribersOfT.Contains(handler))
            {
                return;
            }

            lock (monitor)
            {
                if (subscribersOfT.Contains(handler))
                {
                    return;
                }

                subscribersOfT.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            IList<object> subscribersOfT;

            if (false == subscribers.TryGetValue(typeof(T), out subscribersOfT))
            {
                return;
            }

            subscribersOfT.Remove(handler);
        }

        public static void Publish<T>(T @event)
        {
            IList<object> subscribersOfT;

            if (false == subscribers.TryGetValue(typeof(T), out subscribersOfT))
            {
                return;
            }

            // We'll trigger the delegates from a copy
            // to allow them to unsubscribe directly
            // from the handler.
            var copy = new List<object>(subscribersOfT);

            foreach (var c in copy)
            {
                ((Action<T>) c)(@event);
            }
        }

        private SimpleEventBus()
        {
        }
    }
}

