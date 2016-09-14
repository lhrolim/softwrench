using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cts.commons.Util {
    public static class BaseReflectionUtil {

        public static object GetProperty(object baseObject, string propertyName) {
            var prop = PropertyDescriptor(baseObject, propertyName);
            return prop == null ? null : prop.GetValue(baseObject);
        }

        public static object GetProperty(object baseObject, int index) {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(baseObject)[index];
            return prop == null ? null : prop.GetValue(baseObject);
        }



        public static PropertyDescriptor PropertyDescriptor(object baseObject, string propertyName) {
            var prop = TypeDescriptor.GetProperties(baseObject)[propertyName];
            if (prop == null) {
                prop = TypeDescriptor.GetProperties(baseObject)[propertyName.ToUpper()];
            }
            return prop;
        }
    }
}
