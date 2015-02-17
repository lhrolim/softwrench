using System;
using cts.commons.portable.Util;
using cts.commons.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Preferences {
    public class GridFilterException : Exception {

        public GridFilterException(string message): base(message) {

        }


        public static GridFilterException FilterWithSameAliasAlreadyExists(string alias, string application) {
            return new GridFilterException("Filter {0} already exists for application {1}".Fmt(alias, application));
        }

        public static GridFilterException FilterNotFound(int? id) {
            return new GridFilterException("Filter {0} not found".Fmt(id));
        }

        public static GridFilterException NoPermission() {
            return new GridFilterException("You don´t have permission to delete that filter");
        }
    }
}
