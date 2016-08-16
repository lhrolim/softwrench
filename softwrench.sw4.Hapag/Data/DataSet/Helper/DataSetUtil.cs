using softwrench.sW4.Shared2.Data;
using System;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    class DataSetUtil {
        public static void FillField(AttributeHolder attributeHolder, string field, string attribute) {
            try {
                FillBlank(attributeHolder, field);
                var value = attributeHolder.GetAttribute(attribute);
                if (IsValid(value, typeof(String))) {
                    attributeHolder[field] = value.ToString();
                }
            } catch {
                FillBlank(attributeHolder, field);
            }
        }

        public static void FillBlank(AttributeHolder attributeHolder, string field) {
            attributeHolder[field] = string.Empty;
        }

        public static string GetLast(string source, int tailLength) {
            if (tailLength >= source.Length) {
                return source;
            }
            return source.Substring(source.Length - tailLength);
        }

        public static bool IsValid(object getAttribute, Type type) {
            if (type == typeof(String)) {
                return getAttribute != null && !string.IsNullOrEmpty((string)getAttribute);
            } if (type == typeof(int)) {
                return getAttribute is int;
            }
            return false;
        }
    }
}
