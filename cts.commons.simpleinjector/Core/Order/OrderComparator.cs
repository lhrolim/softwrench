using System.Collections.Generic;
using System.Linq;

namespace cts.commons.simpleinjector.Core.Order {
    [System.Serializable]
    public class OrderComparator<T> : IComparer<T> {

        public static OrderComparator<T> Instance = new OrderComparator<T>();

        public virtual int Compare(T o1, T o2) {
            var p1 = (o1 is IPriorityOrdered);
            var p2 = (o2 is IPriorityOrdered);
            if (p1 && !p2) {
                return -1;
            }
            if (p2 && !p1) {
                return 1;
            }

            var ordered = o1 as IOrdered;
            var ordered2 = o2 as IOrdered;
            var num = (ordered != null) ? ordered.Order : int.MaxValue - 100; // -100 to give space to put ordereds after default
            var num2 = (ordered2 != null) ? ordered2.Order : int.MaxValue - 100; // -100 to give space to put ordereds after default
            if (num < num2) {
                return -1;
            }
            return num > num2 ? 1 : CompareEqualOrder(o1, o2);
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
