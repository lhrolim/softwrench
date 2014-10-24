﻿using Iesi.Collections.Generic;
using NHibernate.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Util {
    public static class CollectionExtensions {

        public static bool IsNullOrEmpty(IEnumerable enumerable) {
            return enumerable == null || !enumerable.Any();
        }

        public static List<TSource> AddReturn<TSource>(this List<TSource> source, TSource item) {
            source.Add(item);
            return source;
        }

        public static Set<TSource> AddReturn<TSource>(this Set<TSource> source, TSource item) {
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
