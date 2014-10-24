using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util {
    public class Validate {

        public static void NotNull(object parameterValue, string parameterName) {
            if (parameterValue == null) {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
