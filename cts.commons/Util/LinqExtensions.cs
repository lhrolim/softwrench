using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;

namespace cts.commons.Util {
    public static class LinqExtensions {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) {
            return new HashSet<T>(source);
        }

        public static HashedSet<T> ToHashedSet<T>(this IEnumerable<T> source) {
            return new HashedSet<T>(source.ToList());
        }
    }
}
