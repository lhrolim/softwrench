using System.Collections.Generic;

namespace cts.commons.portable.Util {
    public static class DictionaryUtil {

        public static bool TryGetValueAsString(this IDictionary<string, object> dict, string key,out string resultst) {
            object value;
            var result = dict.TryGetValue(key, out value);
            resultst = value as string;
            return result;
        }

    }
}
