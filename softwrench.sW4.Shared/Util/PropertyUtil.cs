using System;
using System.Collections.Generic;

namespace softwrench.sW4.Shared.Util {
    public class PropertyUtil {

        public static IDictionary<string, string> ConvertToDictionary(string propertyString) {
            try {
                var result = new Dictionary<string, string>();
                if (propertyString == null) {
                    return result;
                }
                if (propertyString.EndsWith(";")) {
                    propertyString = propertyString.Substring(0, propertyString.Length - 1);
                }
                var strings = propertyString.Split(';');
                foreach (var s in strings) {
                    var propertyArr = s.Split('=');
                    result.Add(propertyArr[0].Trim(), propertyArr[1].Trim());
                }
                return result;
            } catch {
                throw new InvalidOperationException(String.Format("Error parsing property string {0}", propertyString));
            }
        }
    }
}
