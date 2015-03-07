using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using softWrench.sW4.Util;


namespace ctscommons.Util {
    public static class CollectionExtensions {

        public static bool IsNullOrEmpty<T>(IEnumerable<T> enumerable) {
            return enumerable == null || !enumerable.Any();
        }

        public static List<TSource> AddReturn<TSource>(this List<TSource> source, TSource item) {
            source.Add(item);
            return source;
        }

        public static Set<TSource> AddReturn<TSource>(this Iesi.Collections.Generic.Set<TSource> source, TSource item) {
            source.Add(item);
            return source;
        }

        public static void AddAll<TSource>(this System.Collections.Generic.ISet<TSource> source, IEnumerable<TSource> items) {
            foreach (var item in items) {
                source.Add(item);
            }
        }

        public static TSource FirstWithException<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, string msg, params object[] args) {
            var first = source.FirstOrDefault(predicate);
            if (first == null) {
                throw ExceptionUtil.InvalidOperation(msg, args);
            }
            return first;
        }
    }
}
