using System;

namespace cts.commons.Util {
    public class PredicateUtil {

        /// <summary>
        /// Combines the predicates with an 'or' operator and returns the resulting predicate.
        /// source: http://stackoverflow.com/questions/1248232/combine-multiple-predicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public static Predicate<T> Or<T>(params Predicate<T>[] predicates) {
            return delegate (T item) {
                foreach (var predicate in predicates) {
                    if (predicate == null) continue;
                    if (predicate(item)) {
                        return true;
                    }
                }
                return false;
            };
        }

        /// <summary>
        /// Combines the predicates with an 'and' operator and returns the resulting predicate.
        /// source: http://stackoverflow.com/questions/1248232/combine-multiple-predicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public static Predicate<T> And<T>(params Predicate<T>[] predicates) {
            return delegate (T item) {
                foreach (var predicate in predicates) {
                    if (predicate == null) continue;
                    if (!predicate(item)) {
                        return false;
                    }
                }
                return true;
            };
        }

    }
}
