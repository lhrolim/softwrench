using System;
using System.Collections.Generic;
using System.Linq;

namespace cts.commons.portable.Util {
    public class ParsingUtil {

        public static System.Collections.Generic.ISet<String> GetCommaSeparetedParsingResults(string xmlvalue) {
            if (String.IsNullOrEmpty(xmlvalue)) {
                return new HashSet<string>();
            }
            var fields = xmlvalue.Split(',').AsEnumerable();
            return new HashSet<string>(fields);
        }

    }
}
