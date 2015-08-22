using System.Collections.Generic;

namespace softWrench.sW4.Util {
    class CollectionUtil {

        public static List<T> SingleElement<T>(T ob) where T : class {
            return new List<T> { ob };
        }


    }
}
