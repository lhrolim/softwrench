using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Data.Entities;

namespace softWrench.sW4.Util {
    public class ApplicationConfiguration : ISWEventListener<ClientChangeEvent> {
        private const string UnitTestProfile = "unittest";

        private static string _testclientName;


        private static string _clientName;
        private static string _environment;

        private static MaxPropValueDao _maxPropValueDao = new MaxPropValueDao();

        public static string SystemVersion {
            get {
                return ConfigurationManager.AppSettings["version"];
            }
        }

        public static string SystemRevision {
            get {
                var assemblyInfoVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return assemblyInfoVersion.Build.ToString() + "." + assemblyInfoVersion.Revision.ToString();
            }
        }

        public static long SystemBuildDateInMillis {
            get {
                return (long)(SystemBuildDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }

        public static DateTime SystemBuildDate {
            get {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
                return buildDate;
            }
        }

        public static void FixClientName(string clientName) {
            //to be used on dev environments only
            if (!IsDev()) {
                return;
            }
            _clientName = clientName;
        }

        /// <summary>
        /// Name which indentifies a client, to fetch the correct metadata
        /// </summary>
        public static string ClientName {
            get {
                if (_clientName != null) {
                    //caching file system access
                    return _clientName;
                }
                if (_testclientName != null && IsUnitTest) {
                    return _testclientName;
                }
                _clientName = ConfigurationManager.AppSettings["clientkey"];
                return _clientName;
            }
        }

        public static string TestclientName {
            get {
                return _testclientName;
            }
            set {
                _testclientName = value;
            }
        }


        public static readonly string Profile = GetProfile();

        private static string GetProfile() {
            if (_environment != null) {
                return _environment;
            }
            var declaredProfile = ConfigurationManager.AppSettings["profile"];
            _environment = declaredProfile ?? UnitTestProfile;
            return _environment;
        }

        #region MaximoProperties


        /// <summary>
        /// name of this system in maximo
        /// </summary>
        public static string ExternalSystemName {
            get {
                return MetadataProvider.GlobalProperty("externalSystemName", true);
            }
        }

        /// <summary>
        /// Identifies the maximo webservice provider
        /// </summary>
        public static string WsProvider {
            get {
                return WsUtil.WsProvider().ToString().ToLower();
            }
        }



        public static string WsUrl {
            get {
                return MetadataProvider.GlobalProperty("basewsURL", true);
            }
        }

        public static string WfUrl {
            get {
                return MetadataProvider.GlobalProperty("basewfURL", true);
            }
        }

        public static string WsPrefix {
            get {
                return MetadataProvider.GlobalProperty("baseWSPrefix");
            }
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
            get {
                return MetadataProvider.GlobalProperty("ldapServer");
            }
        }

        public static int LdapPortNumber {
            get {
                return Convert.ToInt32(MetadataProvider.GlobalProperty("ldapPortNumber"));
            }
        }

        public static string LdapBaseDn {
            get {
                return MetadataProvider.GlobalProperty("ldapBaseDn");
            }
        }

        #endregion

        #region Mif Credentials

        public static string MifCredentialsUser {
            get {
                return MetadataProvider.GlobalProperty("mifcredentials.user");
            }
        }

        public static string MifCredentialsPassword {
            get {
                return MetadataProvider.GlobalProperty("mifcredentials.password");
            }
        }

        #endregion

        #region Ism Credentials

        public static string IsmCredentialsUser {
            get {
                return MetadataProvider.GlobalProperty("ismcredentials.user");
            }
        }

        public static string IsmCredentialsPassword {
            get {
                return MetadataProvider.GlobalProperty("ismcredentials.password");
            }
        }

        #endregion

        #region ServiceIT Config

        public static string ServiceItLoginPath {
            get {
                return MetadataProvider.GlobalProperty("serviceItLoginPath");
            }
        }

        public static string SertiveItFaqUsefulLinksPath {
            get {
                return MetadataProvider.GlobalProperty("faqusefullinksPath");
            }
        }

        public static string SertiveItSsoServicesQueryPath {
            get {
                return MetadataProvider.GlobalProperty("ssoServicesQueryPath");
            }
        }

        #endregion

        #region Login

        public static string LoginErrorMessage {
            get {
                return MetadataProvider.GlobalProperty("loginErrorMessage");
            }
        }

        public static string LoginUserNameMessage {
            get {
                return MetadataProvider.GlobalProperty("loginUserNameMessage");
            }
        }

        public static string LoginPasswordMessage {
            get {
                return MetadataProvider.GlobalProperty("loginPasswordMessage");
            }
        }

        #endregion

        #region Database default values

        public static string DefaultOrgId {
            get {
                return MetadataProvider.GlobalProperty("defaultOrgId");
            }
        }

        public static string DefaultSiteId {
            get {
                return MetadataProvider.GlobalProperty("defaultSiteId");
            }
        }

        public static string DefaultStoreloc {
            get {
                return MetadataProvider.GlobalProperty("defaultStoreloc");
            }
        }

        #endregion

        #region Change

        internal static string TemplateIdHandler(string templateid) {
            var templateids = templateid.Split(',');
            var strtemplateids = string.Join("','", templateids);
            strtemplateids = "'" + strtemplateids + "'";

            return strtemplateids;
        }

        public static string DefaultChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("defaultChangeTeamplateId");
                return TemplateIdHandler(t);
            }
        }

        public static string SsoChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("ssoChangeTeamplateId");
                return TemplateIdHandler(t);
            }
        }

        public static string TuiChangeTeamplateId {
            get {
                var t = MetadataProvider.GlobalProperty("tuiChangeTeamplateId");
                return TemplateIdHandler(t);
            }
        }

        #endregion

        #region Notification Functionality

        public static bool ActivityStreamFlag {
            get {
                var flagStr = MetadataProvider.GlobalProperty("notifications.activityStream.enabled");
                bool flag;
                bool.TryParse(flagStr, out flag);
                return flag;
            }
        }

        public static string NotificationRefreshRate {
            get {
                var flagValue = MetadataProvider.GlobalProperty("notifications.updateJob.refreshRate");
                var flag = flagValue ?? "2";
                return flag;
            }
        }

        public static string ActivityStreamRefreshRate {
            get {
                var flagValue = MetadataProvider.GlobalProperty("notifications.activityStream.refreshRate");
                var flag = flagValue ?? "2";
                return flag;
            }
        }

        #endregion

        #region UI Options

        public static bool UIShowClassicAdminMenu {
            get {
                var flagStr = MetadataProvider.GlobalProperty("ui.adminmenu.showclassic");
                var flag = false;
                bool.TryParse(flagStr, out flag);
                return flag;
            }
        }

        public static bool UIShowToolbarLabels
        {
            get
            {
                var flagStr = MetadataProvider.GlobalProperty("ui.toolbars.showlabels");
                var flag = false;
                bool.TryParse(flagStr, out flag);
                return flag;
            }
        }

        #endregion

        #region Attachments

        public static string[] AllowedFilesExtensions {
            get {
                // SWWEB-1091 to extract the vbalue out from MAXIMO
                // var ext = MetadataProvider.GlobalProperty("allowedAttachmentExtensions");
                var ext = _maxPropValueDao.GetValue("mxe.doclink.doctypes.allowedFileExtensions");

                if (!string.IsNullOrWhiteSpace(ext)) {
                    return ext.Split(',');
                }

                return new[] { "pdf", "zip", "txt", "jpg", "bmp", "doc", "docx", "dwg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "html", "rtf" };
            }
        }

        /// <summary>
        /// Gets the maximum screenshot size in megabytes
        /// </summary>
        public static int MaxScreenshotSize {
            get {
                var maxScreenshotSizeStr = MetadataProvider.GlobalProperty("maxScreenshotSize");
                int maxScreenshotSizeInt = 5;
                int.TryParse(maxScreenshotSizeStr, out maxScreenshotSizeInt);
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
                int.TryParse(maxAttachmentSizeStr, out maxAttachmentSizeInt);
                return maxAttachmentSizeInt;
            }
        }

        #endregion

        public static int MaximoRequestTimeout {
            get {
                var timeoutStr = MetadataProvider.GlobalProperty("maximoRequestTimeout");
                var timeoutInt = 100000;
                int.TryParse(timeoutStr, out timeoutInt);
                return timeoutInt;
            }
        }

        private static DBMS? _swdbType = null;
        public static DBMS? _maximodbType = null;

        public static bool IsMif() {
            return WsProvider.Equals("mif", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool Is75Maximo() {
            return WsProvider.Equals("mif", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsISM() {
            return WsProvider.Equals("ism", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsUnitTest {
            get {
                return GetProfile() == UnitTestProfile;
            }
        }



        public static long StartTimeMillis {
            get; set;
        }


        public static long GetStartTimeInMillis() {
            return SystemBuildDateInMillis;
        }

        public static bool IsLocal() {
            var baseDirectory = EnvironmentUtil.GetLocalSWFolder();
            var isLocal = File.Exists(baseDirectory + "local.properties");
            return isLocal;
        }

        public static bool IsDev() {
            return Profile.Contains("dev");
        }

        /// <summary>
        /// whether we're using a Pull request dev environment 
        /// </summary>
        /// <returns></returns>
        public static bool IsDevPR() {
            return Profile.Contains("dev_pr");
        }

        public static bool IsQA() {
            return Profile == "qa";
        }

        public static bool IsProd() {
            return Profile.ToLower().StartsWith("prod");
        }

        public static bool IsMea() {
            return !IsMif() && !IsISM();
        }

        public static string DBConnectionString(DBType dbType) {
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
                    //this code is playing safe forcing developers to always configure a local db instead of running the risk of pointing to a wrong instance
                    var localFilePath = EnvironmentUtil.GetLocalSWFolder() + "local.properties";
                    if (new FileInfo(localFilePath).Length != 0) {
                        var stream = new StreamReader(localFilePath);
                        var localProperties = new XmlPropertyMetadataParser().Parse(stream);
                        var overridenLocalDB = localProperties.GlobalProperty("swdb_url");
                        var overridenLocalSwdbpRovider = localProperties.GlobalProperty("swdb_provider");
                        if (overridenLocalDB != null && overridenLocalSwdbpRovider != null) {
                            LoggingUtil.DefaultLog.Info("using customized local database {0} | {1} ".Fmt(overridenLocalDB, overridenLocalSwdbpRovider));
                            return new ConnectionStringSettings("swdb", overridenLocalDB, overridenLocalSwdbpRovider);
                        }
                    }
                    var swdbConnectionString = ConfigurationManager.ConnectionStrings["swdb"];
                    if (swdbConnectionString == null) {
                        swdbConnectionString = new ConnectionStringSettings("swdb", "Server=localhost;Database=swdb;Uid=sw;Pwd=sw;", "MySql.Data.MySqlClient");
                    }
                    return swdbConnectionString;
                }
                //need to assure that we use the real swdb_urls, in order for it to work on pr environments
                var url = MetadataProvider.GlobalProperty(MetadataProperties.SWDBUrl, true,false,true);
                var provider = MetadataProvider.GlobalProperty(MetadataProperties.SWDBProvider, true,false, true);


                return new ConnectionStringSettings("swdb", url, provider);
            }
        }

        public static DBConnectionResult DBConnectionStringBuilder(DBType dbType) {
            var dbConnectionString = DBConnectionString(dbType);
            if (IsMSSQL(dbType) || IsOracle(dbType)) {
                var sqlBuilder = new SqlConnectionStringBuilder(dbConnectionString);
                return new DBConnectionResult {
                    Catalog = sqlBuilder.InitialCatalog,
                    DataSource = sqlBuilder.DataSource
                };
            }
            var builder = new DbConnectionStringBuilder() { ConnectionString = dbConnectionString };
            if (IsDB2(dbType)) {
                return new DBConnectionResult {
                    Catalog = (string)builder["Server"],
                    DataSource = (string)builder["Database"],
                    Schema = (string)builder["CurrentSchema"]
                };
            }

            return new DBConnectionResult {
                Catalog = (string)builder["database"],
                DataSource = (string)builder["server"]
            };


        }

        public static string UIDName(DBType dbType) {
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

        public static bool IsMySql() {
            var toUse = ToUse(DBType.Swdb);
            if (toUse == null) {
                toUse = DiscoverDBMS(DBType.Swdb);
            }
            return toUse == DBMS.MYSQL;
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

        public static bool IsOracle(DBType dbType) {
            DBMS? toUse = dbType == DBType.Maximo ? _maximodbType : _swdbType;
            if (toUse == null) {
                toUse = DiscoverDBMS(dbType);
            }
            return toUse == DBMS.ORACLE;
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
                case "Oracle.DataAccess.Client":
                return DBMS.ORACLE;
                case "IBM.Data.DB2":
                return DBMS.DB2;
            }
            return DBMS.MYSQL;
        }


        public static bool IsLocalHostSWDB() {
            var connString = DBConnectionString(DBType.Swdb);
            return connString.Contains("localhost");
        }

        public void HandleEvent(ClientChangeEvent eventToDispatch) {
            _clientName = null;
            _swdbType = null;
            _environment = null;
            _maximodbType = null;
        }

        public static bool IsClient(string clientName) {
            return ClientName.Equals(clientName);
        }
    }

}
