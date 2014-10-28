using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util {
    class CollectionUtil {

        public static List<T> SingleElement<T>(T ob) where T : class {
            return new List<T> { ob };
        }


    }
}
