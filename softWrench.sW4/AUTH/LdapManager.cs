using System;
using System.DirectoryServices;
using cts.commons.simpleinjector;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;


namespace softWrench.sW4.AUTH {
    public class LdapManager : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(LdapManager));

        private IConfigurationFacade _facade;
        private readonly SWDBHibernateDAO _dao;

        public LdapManager(IConfigurationFacade facade, SWDBHibernateDAO dao) {
            Log.Debug("Creating LdapManager");
            _facade = facade;
            _dao = dao;
        }

        private LdapAuthArgs GetArgs(string userName, string password) {
            //TODO: may switch LDAP configuration to Config Application here!
            var server = ApplicationConfiguration.LdapServer;
            var port = ApplicationConfiguration.LdapPortNumber;
            var baseDn = ApplicationConfiguration.LdapBaseDn;

            const string msg = "LDAP args are:\nServer {0}\nPort {1}\nBase DN {2}\nUsername {3}";
            Log.Debug(string.Format(msg, server, port, baseDn, userName));

            return new LdapAuthArgs(server, port, baseDn, userName, password);
        }

        public Boolean IsLdapSetup() {
            var isLdapSetup = ApplicationConfiguration.LdapServer != null;
            Log.Debug(isLdapSetup ? "LDAP is set up." : "LDAP is not set up");
            return isLdapSetup;
        }

        public LdapAuthResult LdapAuth(String userName, string password) {
            Log.Info(string.Format("LDAP auth try with '{0}' username", userName));
            var args = GetArgs(userName, password);
            try {
                Log.Info(string.Format("LDAP connection string: {0}", args.ConnectionString));
                var objDirEntry = new DirectoryEntry(args.ConnectionString, args.UserName, args.Password);

                var search = new DirectorySearcher(objDirEntry) { Filter = "(objectClass=*)" };

                var result = search.FindOne();
                if (null == result) {
                    return new LdapAuthResult(false, "user not found");
                }
            } catch (Exception ex) {
                Log.Warn(ex.Message,ex);
                return new LdapAuthResult(false, ex.Message);
            }
            return new LdapAuthResult(true, null);
        }

        public class LdapAuthResult {
            public bool Success;
            public string LdapMsg;

            public LdapAuthResult(bool success, string ldapMsg) {
                Success = ApplicationConfiguration.IsLocal() || success;
                LdapMsg = ldapMsg;
                var msg = Success ? "LDAP auth succeeded with message: {0}" : "LDAP auth failed with message: {0}";
                Log.Debug(string.Format(msg, LdapMsg));
            }
        }


    }
}
