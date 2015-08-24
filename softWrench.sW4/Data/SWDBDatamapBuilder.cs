using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data {
    public class SWDBDatamapBuilder {
        public static DataMap BuildDataMap(string application, object typedObject, ApplicationSchemaDefinition schema) {
            var fields = new Dictionary<string, object>();

            foreach (var displayable in schema.Displayables) {
                if (displayable is IApplicationAttributeDisplayable) {
                    var attr = ((IApplicationAttributeDisplayable)displayable).Attribute;
                    if (attr != null) {
                        fields[attr] = ReflectionUtil.GetProperty(typedObject, attr.Replace("#", ""));
                    }
                }
            }
            return new DataMap(application, fields, null, true);
        }
    }
}
