using System;

namespace softWrench.sW4.Util {
    public class SwConstants {
        public static readonly string ExternalSystemName = ApplicationConfiguration.ExternalSystemName;
        public static readonly string BaseWsPrefix = ApplicationConfiguration.WsUrl;
        public static readonly string WsProvider = ApplicationConfiguration.WsProvider;

        public const string AUTH_LOG = "AUTH.LOG";
        public const string SQL_LOG = "MAXIMO.SQL";
        public const string NHIBERNATE_LOG = "NHIBERNATE.SQL";
        public const string SQLDB_LOG = "SWDB.SQL";
        public const string JOB_LOG = "JOB.LOG";
        public const string DATETIME_LOG = "DATETIME.LOG";

        public static Boolean IsMif() {
            return WsProvider == "mif";
        }

    }
}
