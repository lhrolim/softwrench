using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.user.classes.entities.security {
    public static class SecurityEntityExtensions {

        public static string Label(this SchemaPermissionMode mode) {
            switch (mode) {
                case SchemaPermissionMode.Grid:
                return "Grid";
                case SchemaPermissionMode.Creation:
                return "Creation";
                case SchemaPermissionMode.Update:
                return "Update";
                case SchemaPermissionMode.View:
                return "View";
                default:
                return "";
            }
        }


        public static int Priority(this SchemaPermissionMode mode) {
            switch (mode) {
                case SchemaPermissionMode.Grid:
                return 0;
                case SchemaPermissionMode.Creation:
                return 2;
                case SchemaPermissionMode.Update:
                return 3;
                case SchemaPermissionMode.View:
                return 4;
                default:
                return 0;
            }
        }
    }
}
