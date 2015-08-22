using System;
using System.ComponentModel;

namespace softWrench.sW4.Security.Init {
    static class RoleExtensions {
        public static string GetName(this Enum value) {
            return value.ToString().ToLower();
        }

        public static string GetDescription(this Enum value) {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null) {
                var field = type.GetField(name);
                if (field != null) {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null) {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}
