using System;

namespace softWrench.sW4.Configuration.Services.Api {



    public enum ConfigTypes {


        Global, WhereClauses, DashBoards
    }

    public static class ConfigTypesExtension {

        public const string WcConfigPrefix = "_whereclauses";

        public static String GetRootLevel(this ConfigTypes type) {
            if (ConfigTypes.Global == type) {
                return "Global";
            }
            if (ConfigTypes.WhereClauses == type) {
                return WcConfigPrefix;
            }
            if (ConfigTypes.DashBoards == type) {
                return "_dashboards";
            }
            throw new InvalidOperationException();
        }
    }
}