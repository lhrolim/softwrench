using Iesi.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections;

namespace softWrench.sW4.Util {

    public static class IesiCollectionExtensions  {


        public static Set<TSource> AddReturn<TSource>(this Set<TSource> source, TSource item) {
            source.Add(item);
            return source;
        }

        public static void AddAll<TSource>(this ISet source, IEnumerable<TSource> items) {
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
