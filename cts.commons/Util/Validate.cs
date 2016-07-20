using System;
using System.Collections.Generic;
using System.Linq;

namespace cts.commons.Util {
    public class Validate {

        public static void NotNull(object parameterValue, string parameterName) {
            if (parameterValue == null) {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NotEmpty(string value, string name) {
            if(string.IsNullOrEmpty(value)) throw new ArgumentException("Empty or Null string", name);
        }

        public static void NotEmpty<T>(IEnumerable<T> value, string name) {
            if (value == null) throw new ArgumentNullException(name);
            if(!value.Any()) throw new ArgumentException("Empty or Null Enumerable", name);
        }
    }
}
