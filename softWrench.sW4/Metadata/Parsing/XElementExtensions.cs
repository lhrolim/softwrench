using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Parsing {
    static class XElementExtensions {
        public static String LocalName(this XElement element) {
            return element.Name.LocalName;
        }

        public static Boolean IsNamed(this XElement element, string name) {
            return element.Name.LocalName.Equals(name);
        }

        public static String AttributeValue(this XElement element, string attribute, bool required = false) {
            var result = element.Attribute(attribute).ValueOrDefault((string)null);
            if (required && result == null) {
                throw new InvalidOperationException("attribute {0} is required".Fmt(attribute));
            }
            return result;
        }
    }
}