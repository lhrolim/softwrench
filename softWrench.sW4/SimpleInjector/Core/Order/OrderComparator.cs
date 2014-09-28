using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.SimpleInjector.Core.Order {
    [System.Serializable]
    public class OrderComparator<T> : System.Collections.Generic.IComparer<T> {

        public static OrderComparator<T> Instance = new OrderComparator<T>();

        public virtual int Compare(T o1, T o2) {
            var ordered = o1 as IOrdered;
            var ordered2 = o2 as IOrdered;
            var num = (ordered != null) ? ordered.Order : 2147483647;
            var num2 = (ordered2 != null) ? ordered2.Order : 2147483647;
            if (num < num2) {
                return -1;
            }
            return num > num2 ? 1 : this.CompareEqualOrder(o1, o2);
        }
        protected virtual int CompareEqualOrder(object o1, object o2) {
            return 0;
        }

        public static void Sort(List<T> list) {
            if (list.Count() > 1) {
                list.Sort(Instance);
            }
        }

    }
}
