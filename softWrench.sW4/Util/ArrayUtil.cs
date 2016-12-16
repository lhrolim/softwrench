using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Util {
    public class ArrayUtil {

        public static T[] Push<T>(T[] arr, T element) {
            if (arr == null) {
                return new T[] { element };
            }
            var list = arr.ToList();
            list.Add(element);
            return list.ToArray();
        }

        public static T[] PushRange<T>(T[] arr, List<T> element) {
            if (arr == null) {
                return element.ToArray();
            }
            var list = arr.ToList();
            list.AddRange(element);
            return list.ToArray();
        }
    }
}
