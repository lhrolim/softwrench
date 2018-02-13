using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Entities;
using static System.Int16;

namespace softWrench.sW4.Util {
    public class ApplicationConfiguration : ISWEventListener<ClientChangeEvent> {
        private const string UnitTestProfile = "unittest";

        private static string _testclientName;


        private static string _clientName;
        private static string _environment;
        private static readonly DateTime _upTime = DateTime.Now;



        private static readonly IDictionary<DBType, ConnectionStringSettings> _connectionStringCache = new Dictionary<DBType, ConnectionStringSettings>();

        public static string SystemVersion => ConfigurationManager.AppSettings["version"];


        public static string SystemVersionIgnoreDash {
            get {
                var version = SystemVersion;
                var dashIndex = version.IndexOf("-", StringComparison.Ordinal);
                var systemVersion = dashIndex > 0 ? version.Substring(0, dashIndex) : version;
                return systemVersion;
            }
        }


        public static string SystemRevision {
            get {
                var assemblyInfoVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return assemblyInfoVersion.Build.ToString() + "." + assemblyInfoVersion.Revision.ToString();
            }
        }

        public static long SystemBuildDateInMillis => (long)(SystemBuildDate - new DateTime(1970, 1, 1)).TotalMilliseconds;

        public static DateTime SystemBuildDate {
            get {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
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
                if (_testclientName != null && IsUnitTest) {
                    return _testclientName;
                }
                if (_clientName != null) {
                    //caching file system access
                    return _clientName;
                }
                _clientName = ConfigurationManager.AppSettings["clientkey"];
                return _clientName;
            }
        }

        public static object ClientLogFolder {
            get {
                if (ConfigurationManager.AppSettings["clientlogkey"] != null) {
                    return ConfigurationManager.AppSettings["clientlogkey"];
                }
                return ClientName;
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

        public static readonly string SecondProfile = GetProfile("secondprofile");

        private static string GetProfile(string profileKey = "profile") {
            if (_environment != null) {
                return _environment;
            }
            var declaredProfile = ConfigurationManager.AppSettings[profileKey];
            _environment = declaredProfile ?? UnitTestProfile;
            return _environment;
        }


        public static int NhibernateCommandTimeout {
            get {
                var declaredtimeout = ConfigurationManager.AppSettings["nhibernatecommandtimeout"];
                return declaredtimeout == null ? 30 : Parse(declaredtimeout);
            }
        }

        #region MaximoProperties


        /// <summary>
        /// name of this system in maximo
        /// </summary>
        public static string ExternalSystemName => MetadataProvider.GlobalProperty("externalSystemName", true);





        public static string WfUrl => MetadataProvider.GlobalProperty("basewfURL", true);

        public static string WsPrefix => MetadataProvider.GlobalProperty("baseWSPrefix");


        #endregion


        #region Ism Credentials

        public static string IsmCredentialsUser => MetadataProvider.GlobalProperty("ismcredentials.user");

        public static string IsmCredentialsPassword => MetadataProvider.GlobalProperty("ismcredentials.password");

        #endregion

        #region ServiceIT Config

        public static string ServiceItLoginPath => MetadataProvider.GlobalProperty("serviceItLoginPath");

        public static string SertiveItFaqUsefulLinksPath => MetadataProvider.GlobalProperty("faqusefullinksPath");

        public static string SertiveItSsoServicesQueryPath => MetadataProvider.GlobalProperty("ssoServicesQueryPath");

        #endregion

        #region Login

        public static string LoginErrorMessage => MetadataProvider.GlobalProperty("loginErrorMessage");

        public static string LoginUserNameMessage => MetadataProvider.GlobalProperty("loginUserNameMessage");

        public static string LoginPasswordMessage => MetadataProvider.GlobalProperty("loginPasswordMessage");

        #endregion

        #region Database default values

        //TODO: remove these static calls

        public static string GetPropertyHandlingTestScenario(string configKey, string propertyName) {
            if (SimpleInjectorGenericFactory.Instance != null && !IsUnitTest) {
                //some unit tests might not have an instance defined
                return SimpleInjectorGenericFactory.Instance.GetObject<IConfigurationFacade>().Lookup<string>(configKey, propertyName);
            }
            return MetadataProvider.GlobalProperty(propertyName);
        }

        /// <summary>
        /// Identifies the maximo webservice provider
        /// </summary>
        public static string WsProvider => WsUtil.WsProvider().ToString().ToLower();

        public static string DefaultOrgId => GetPropertyHandlingTestScenario(ConfigurationConstants.Maximo.DefaultOrgId, "defaultOrgId");

        public static string DefaultSiteId => GetPropertyHandlingTestScenario(ConfigurationConstants.Maximo.DefaultSiteId, "defaultSiteId");

        public static string DefaultStoreloc => GetPropertyHandlingTestScenario(ConfigurationConstants.Maximo.DefaultStoreLoc, "defaultStoreloc");

        #endregion

        #region HapagChange

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

        public static bool CrudSearchFlag {
            get; set;
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
                bool flag;
                bool.TryParse(flagStr, out flag);
                return flag;
            }
        }

        public static bool UIShowToolbarLabels => !"false".EqualsIc(MetadataProvider.GlobalProperty("ui.toolbars.showlabels"));

        #endregion

        #region Attachments


        /// <summary>
        /// Gets the maximum screenshot size in megabytes
        /// </summary>
        public static int MaxScreenshotSize {
            get {
                var maxScreenshotSizeStr = MetadataProvider.GlobalProperty("maxScreenshotSize");
                var maxScreenshotSizeInt = 5;
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
                var maxAttachmentSizeInt = 10;
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

        public static bool IsUnitTest => GetProfile() == UnitTestProfile;


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

        public static bool UseDevScriptsAndStyles() {
            return IsLocal();
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
        public static bool IsUat() {
            return Profile == "uat";
        }

        public static bool IsProd() {
            return Profile.ToLower().StartsWith("prod");
        }

        public static bool IsMea() {
            return !IsMif() && !IsISM();
        }

        public static bool IsSCCD() {
            var source = MetadataProvider.GlobalProperty(MetadataProperties.Source) ?? "";
            return source.ToLower() == "smartcloud7.5";
        }

        public static bool Is76() {
            var source = MetadataProvider.GlobalProperty(MetadataProperties.Source) ?? "";
            return source.ToLower() == "maximo7.6";
        }

        public static string DBConnectionString(DBType dbType) {
            var connectionStringSettings = DBConnection(dbType);
            return connectionStringSettings == null ? null : connectionStringSettings.ConnectionString;
        }


        public static ConnectionStringSettings DBConnection(DBType dbType) {
            if (_connectionStringCache.ContainsKey(dbType)) {
                return _connectionStringCache[dbType];
            }

            if (dbType == DBType.Maximo) {
                var url = MetadataProvider.GlobalProperty(MetadataProperties.MaximoDBUrl, true);
                var provider = MetadataProvider.GlobalProperty(MetadataProperties.MaximoDBProvider, true);
                var settings = new ConnectionStringSettings("maximo", url, provider);
                _connectionStringCache.Add(dbType, settings);
                return settings;
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
                            var settings = new ConnectionStringSettings("swdb", overridenLocalDB, overridenLocalSwdbpRovider);
                            _connectionStringCache.Add(dbType, settings);
                            return settings;
                        }
                    }
                    var swdbConnectionString = ConfigurationManager.ConnectionStrings["swdb"];
                    if (swdbConnectionString == null) {
                        swdbConnectionString = new ConnectionStringSettings("swdb", "Server=localhost;Database=swdb;Uid=sw;Pwd=sw;", "MySql.Data.MySqlClient");
                        _connectionStringCache.Add(dbType, swdbConnectionString);
                    }
                    return swdbConnectionString;
                }
                //need to assure that we use the real swdb_urls, in order for it to work on pr environments
                var url = MetadataProvider.GlobalProperty(MetadataProperties.SWDBUrl, true, false, true);
                var provider = MetadataProvider.GlobalProperty(MetadataProperties.SWDBProvider, true, false, true);


                var connection = new ConnectionStringSettings("swdb", url, provider);
                _connectionStringCache.Add(dbType, connection);
                return connection;
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
            var toUse = dbType == DBType.Maximo ? _maximodbType : _swdbType;
            if (toUse == null) {
                toUse = DiscoverDBMS(dbType);
            }
            return toUse == DBMS.DB2;
        }

        public static bool IsOracle(DBType dbType) {
            var toUse = dbType == DBType.Maximo ? _maximodbType : _swdbType;
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
            _connectionStringCache.Clear();
        }

        public static bool IsClient(string clientName) {
            return ClientName.Equals(clientName);
        }

        public static DateTime UpTime => _upTime;

    }

}
