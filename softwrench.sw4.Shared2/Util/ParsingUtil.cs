using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util {
    public class ParsingUtil {

        public static ISet<String> GetCommaSeparetedParsingResults(string xmlvalue) {
            if (String.IsNullOrEmpty(xmlvalue)) {
                return new HashSet<string>();
            }
            var fields = xmlvalue.Split(',').AsEnumerable();
            return new HashSet<string>(fields);
        }

    }
}
