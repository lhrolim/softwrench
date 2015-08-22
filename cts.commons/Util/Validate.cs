using System;

namespace cts.commons.Util {
    public class Validate {

        public static void NotNull(object parameterValue, string parameterName) {
            if (parameterValue == null) {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
