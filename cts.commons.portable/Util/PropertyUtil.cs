using System;
using System.Collections.Generic;

namespace cts.commons.portable.Util {
    public class PropertyUtil {


        public static IDictionary<string, object> ConvertToDictionary(string propertyString, Boolean throwException = true) {
            try {
                var result = new Dictionary<string, object>();
                if (propertyString == null) {
                    return result;
                }
                if (propertyString.EndsWith(";")) {
                    propertyString = propertyString.Substring(0, propertyString.Length - 1);
                }
                var strings = propertyString.Split(';');
                foreach (String s in strings) {
                    var equals = s.IndexOf("=", StringComparison.Ordinal);
                    if (equals == -1) {
                        continue;
                    }
                    //TODO: Make key lower cases... 
                    var beforeEqual = s.Substring(0, @equals).Trim();
                    var afterEqual = s.Substring(@equals + 1, s.Length - @equals - 1).Trim();
                    result.Add(beforeEqual, afterEqual);
                }
                return result;
            } catch {
                if (throwException) {
                    throw new InvalidOperationException(String.Format("Error parsing property string {0}", propertyString));
                }
                return new Dictionary<string, object>();
            }
        }
    }
}
