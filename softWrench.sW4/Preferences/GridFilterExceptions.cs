using System;
using softWrench.sW4.Util;

namespace softWrench.sW4.Preferences {
    public class GridFilterException : Exception {

        public GridFilterException(string message)
            : base(message) {

        }


        public static GridFilterException FilterWithSameAliasAlreadyExists(string alias, string application) {
            return new GridFilterException("Filter {0} already exists for application {1}".Fmt(alias, application));
        }
    }
}
