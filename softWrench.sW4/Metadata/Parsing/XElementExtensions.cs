using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace softWrench.sW4.Metadata.Parsing {
    static class XElementExtensions {
        public static String LocalName(this XElement element) {
            return element.Name.LocalName;
        }

        public static Boolean IsNamed(this XElement element,string name) {
            return element.Name.LocalName.Equals(name);
        }
    }
}
