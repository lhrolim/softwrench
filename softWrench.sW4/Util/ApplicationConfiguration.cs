using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Properties;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace softWrench.sW4.Util {
    public class ApplicationConfiguration {
        private const string UnitTestProfile = "unittest";

        private static string _testclientName;

        private static Version _version;

        public static String SystemVersion {
            get { return ConfigurationManager.AppSettings["version"]; }
        }

        public static String SystemRevision {
            get {
                var assemblyInfoVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return assemblyInfoVersion.Build.ToString() + "." + assemblyInfoVersion.Revision.ToString();
            }
        }

        public static long SystemBuildDateInMillis {
            get { return (long)(SystemBuildDate - new DateTime(1970, 1, 1)).TotalMilliseconds; }
        }

        public static DateTime SystemBuildDate {
            get {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
                return buildDate;
            }
        }

        /// <summary>
        /// Name which indentifies a client, to fetch the correct metadata
        /// </summary>
        public static string ClientName {
            get {

                if (_testclientName != null && IsUnitTest) {
                    return _testclientName;
                }
                return ConfigurationManager.AppSettings["clientkey"];
            }
        }

        public static string TestclientName {
            get { return _testclientName; }
            set { _testclientName = value; }
        }


        public static readonly string Profile = GetProfile();

        private static string GetProfile() {
            string declaredProfile = ConfigurationManager.AppSettings["profile"];
            return declaredProfile ?? UnitTestProfile;
        }

        #region MaximoProperties


        /// <summary>
        /// name of this system in maximo
        /// </summary>
        public static string ExternalSystemName {
            get { return MetadataProvider.GlobalProperty("externalSystemName", true); }
        }

        /// <summary>
        /// Identifies the maximo webservice provider
        /// </summary>
        public static string WsProvider {
            get { return WsUtil.WsProvider().ToString().ToLower(); }
        }

        public static string WsUrl {
            get { return MetadataProvider.GlobalProperty("basewsURL", true); }
        }

        public static string WsPrefix {
            get { return MetadataProvider.GlobalProperty("baseWSPrefix"); }
        }

        public static bool IgnoreWsCertErrors {
            get {
                var strIgnoreWsCertErrors = MetadataProvider.GlobalProperty("ignoreWsCertErrors");
                return "true".Equals(strIgnoreWsCertErrors, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        #endregion

        #region Ldap

        public static string LdapServer {
            get { return MetadataProvider.GlobalProperty("ldapServer"); }
        }

        public static int LdapPortNumber {
            get { return Convert.ToInt32(MetadataProvider.GlobalProperty("ldapPortNumber")); }
        }

        public static string LdapBaseDn {
            get { return MetadataProvider.GlobalProperty("ldapBaseDn"); }
        }

        #endregion

        #region Mif Credentials

        public static string MifCredentialsUser {
            get { return MetadataProvider.GlobalProperty("mifcredentials.user"); }
        }

        public static string MifCredentialsPassword {
            get { return MetadataProvider.GlobalProperty("mifcredentials.password"); }
        }

        #endregion

        #region Ism Credentials

        public static string IsmCredentialsUser {
            get { return MetadataProvider.GlobalProperty("ismcredentials.user"); }
        }

        public static string IsmCredentialsPassword {
            get { return MetadataProvider.GlobalProperty("ismcredentials.password"); }
        }

        #endregion

        #region ServiceIT Config

        public static string ServiceItLoginPath {
            get { return MetadataProvider.GlobalProperty("serviceItLoginPath"); }
        }

        public static string SertiveItFaqUsefulLinksPath {
            get { return MetadataProvider.GlobalProperty("faqusefullinksPath"); }
        }

        public static string SertiveItSsoServicesQueryPath {
            get { return MetadataProvider.GlobalProperty("ssoServicesQueryPath"); }
        }

        public static string SertiveItTuiServicesQueryPath {
            get { return MetadataProvider.GlobalProperty("tuiServicesQueryPath"); }
        }

        #endregion

        #region Login

        public static string LoginErrorMessage {
            get { return MetadataProvider.GlobalProperty("loginErrorMessage"); }
        }

        public static string LoginUserNameMessage {
            get { return MetadataProvider.GlobalProperty("loginUserNameMessage"); }
        }

        public static string LoginPasswordMessage {
            get { return MetadataProvider.GlobalProperty("loginPasswordMessage"); }
        }

        #endregion

        #region Database default values

        public static string DefaultOrgId {
            get { return MetadataProvider.GlobalProperty("defaultOrgId"); }
        }

        public static string DefaultSiteId {
            get { return MetadataProvider.GlobalProperty("defaultSiteId"); }
        }

        #endregion

        #region Change

        public static string[] DefaultChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("defaultChangeTeamplateId");
                return t.Split(',');
            }
        }

        public static string[] SsoChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("ssoChangeTeamplateId");
                return t.Split(',');
            }
        }

        public static string[] TuiChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("tuiChangeTeamplateId");
                return t.Split(',');
            }
        }

        #endregion

        #region Attachments

        public static string[] AllowedFilesExtensions {
            get {
                var ext = MetadataProvider.GlobalProperty("allowedAttachmentExtensions");
                if (!String.IsNullOrWhiteSpace(ext)) {
                    return ext.Split(',');
                } else {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Gets the maximum screenshot size in megabytes
        /// </summary>
        public static int MaxScreenshotSize {
            get {
                var maxScreenshotSizeStr = MetadataProvider.GlobalProperty("maxScreenshotSize");
                int maxScreenshotSizeInt = 5;
                Int32.TryParse(maxScreenshotSizeStr, out maxScreenshotSizeInt);
                return maxScreenshotSizeInt;
            }
        }

        /// <summary>
        /// Gets the maximum attachment size in megabytes
        /// </summary>
        public static int MaxAttachmentSize {
            get {
                var maxAttachmentSizeStr = MetadataProvider.GlobalProperty("maxAttachmentSize");
                int maxAttachmentSizeInt = 10;
                Int32.TryParse(maxAttachmentSizeStr, out maxAttachmentSizeInt);
                return maxAttachmentSizeInt;
            }
        }

        #endregion

        public static Int32 MaximoRequestTimeout {
            get {
                var timeoutStr = MetadataProvider.GlobalProperty("maximoRequestTimeout");
                var timeoutInt = 100000;
                Int32.TryParse(timeoutStr, out timeoutInt);
                return timeoutInt;
            }
        }

        public static DBMS? _swdbType = null;
        public static DBMS? _maximodbType = null;

        public static Boolean IsMif() {
            return WsProvider.Equals("mif", StringComparison.CurrentCultureIgnoreCase);
        }

        public static Boolean IsISM() {
            return WsProvider.Equals("ism", StringComparison.CurrentCultureIgnoreCase);
        }

        public static Boolean IsUnitTest {
            get { return GetProfile() == UnitTestProfile; }
        }

        public static long StartTimeMillis {
            get {
                return (long)(StartDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }

        public static DateTime StartDate { get; set; }

        public static Boolean IsLocal() {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\";
            var isLocal = File.Exists(baseDirectory + "local.properties");
            return isLocal;
        }

        public static Boolean IsDev() {
            return Profile == "dev";
        }

        public static Boolean IsQA() {
            return Profile == "qa";
        }

        public static Boolean IsProd() {
            return Profile.ToLower().StartsWith("prod");
        }

        public static Boolean IsMea() {
            return !IsMif() && !IsISM();
        }

        public static String DBConnectionString(DBType dbType) {
            var connectionStringSettings = DBConnection(dbType);
            return connectionStringSettings == null ? null : connectionStringSettings.ConnectionString;
        }


        public static ConnectionStringSettings DBConnection(DBType dbType) {
            if (dbType == DBType.Maximo) {
                var url = MetadataProvider.GlobalProperty(MetadataProperties.MaximoDBUrl, true);
                string provider = MetadataProvider.GlobalProperty(MetadataProperties.MaximoDBProvider, true);
                return new ConnectionStringSettings("maximo", url, provider);
            } else {
                if (IsLocal() && IsDev()) {
                    //                    var swdbConnectionString = ConfigurationManager.ConnectionStrings["swdb_hapag"];
                    //                    if (swdbConnectionString == null) {
                    var swdbConnectionString = new ConnectionStringSettings("swdb", "Data Source=localhost;Initial Catalog=swdbhapag;User Id=sw;password=sw;", "System.Data.SQL");
                    //                    }
                    return swdbConnectionString;
                }
                var url = MetadataProvider.GlobalProperty(MetadataProperties.SWDBUrl, true);
                var provider = MetadataProvider.GlobalProperty(MetadataProperties.SWDBProvider, true);
                var connectionStringSettings = new ConnectionStringSettings("swdb", url, provider);
                return connectionStringSettings;
            }
        }

        public static DBConnectionResult DBConnectionStringBuilder(DBType dbType) {
            var dbConnectionString = DBConnectionString(dbType);
            if (IsMSSQL(dbType)) {
                var sqlBuilder = new SqlConnectionStringBuilder(dbConnectionString);
                return new DBConnectionResult {
                    Catalog = sqlBuilder.InitialCatalog,
                    DataSource = sqlBuilder.DataSource
                };
            }
            var builder = new DbConnectionStringBuilder() { ConnectionString = dbConnectionString };
            if (IsDB2(dbType)) {
                var dbConnectionResult = new DBConnectionResult {
                    Catalog = (string)builder["Server"],
                    DataSource = (string)builder["Database"],
                    Schema = (string)builder["CurrentSchema"]
                };
                return dbConnectionResult;
            }
            return new DBConnectionResult {
                Catalog = (string)builder["database"],
                DataSource = (string)builder["server"]
            };


        }

        public static String UIDName(DBType dbType) {
            var db = DBConnectionString(dbType);
            var idx = 0;
            if (IsDB2(dbType)) {
                idx = db.IndexOf("UID=", System.StringComparison.Ordinal);
            } else if (IsMSSQL(dbType)) {
                idx = db.IndexOf("User Id=", System.StringComparison.Ordinal);
            }
            var nextIdx = db.IndexOf(';', idx);
            return db.Substring(idx, nextIdx - idx).Split('=')[1];
        }

        public static bool IsMSSQL(DBType dbType) {
            var toUse = ToUse(dbType);
            if (toUse == null) {
                toUse = DiscoverDBMS(dbType);
            }
            return toUse == DBMS.MSSQL;
        }

        internal static DBMS? ToUse(DBType dbType) {
            var dbms = dbType == DBType.Maximo ? _maximodbType : _swdbType;
            if (dbms == null) {
                dbms = DiscoverDBMS(dbType);
            }
            return dbms;
        }

        public static bool IsDB2(DBType dbType) {
            DBMS? toUse = dbType == DBType.Maximo ? _maximodbType : _swdbType;
            if (toUse == null) {
                toUse = DiscoverDBMS(dbType);
            }
            return toUse == DBMS.DB2;
        }


        //TODO: review connection string provider names
        internal static DBMS DiscoverDBMS(DBType dbType) {
            var connectionStringSettings = DBConnection(dbType);
            if (IsUnitTest) {
                //may be null on unit tests
                return DBMS.MSSQL;

            }
            var type = connectionStringSettings.ProviderName;
            switch (type) {
                case "System.Data.SQL":
                    return DBMS.MSSQL;
                case "System.Data.SqlClient":
                    return DBMS.MSSQL;
                case "System.Data.OracleClient":
                    return DBMS.ORACLE;
                case "IBM.Data.DB2":
                    return DBMS.DB2;
            }
            return DBMS.MYSQL;
        }

        public enum DBMS {
            MSSQL, DB2, ORACLE, MYSQL
        }
        public enum DBType {
            Maximo, Swdb
        }


        public static bool IsLocalHostSWDB() {
            var connString = DBConnectionString(DBType.Swdb);
            return connString.Contains("localhost");
        }
    }

}
