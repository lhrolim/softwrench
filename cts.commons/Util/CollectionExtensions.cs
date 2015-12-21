using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Util {
    public static class CollectionExtensions {

        public static bool IsNullOrEmpty<T>(IEnumerable<T> enumerable) {
            return enumerable == null || !enumerable.Any();
        }

        public static List<TSource> AddReturn<TSource>(this List<TSource> source, TSource item) {
            source.Add(item);
            return source;
        }


        public static void AddAll<TSource>(this System.Collections.Generic.ISet<TSource> source, IEnumerable<TSource> items) {
            foreach (var item in items) {
                source.Add(item);
            }
        }
    }
}
