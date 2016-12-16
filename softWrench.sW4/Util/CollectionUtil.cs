using System.Collections.Generic;
using NHibernate.Util;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Util {
    public class CollectionUtil {

        public static List<T> SingleElement<T>(T ob) where T : class {
            return new List<T> { ob };
        }


        public static bool NullOrEmpty(IEnumerable<MenuBaseDefinition> leafs) {
            return leafs == null || leafs.Any();
        }
    }
}
