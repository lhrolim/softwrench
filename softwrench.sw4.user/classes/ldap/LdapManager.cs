using System;
using System.DirectoryServices;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using log4net;
using softwrench.sw4.api.classes.configuration;
using softwrench.sw4.user.classes.config;
using softwrench.sw4.user.classes.services;
using softWrench.sW4.AUTH;

namespace softwrench.sw4.user.classes.ldap {
    public class LdapManager : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(LdapManager));

        private static IApplicationConfiguration _applicationConfiguration;
        private readonly IConfigurationFacadeCommons _facade;
        private readonly UserPasswordService _userPasswordService;

        public LdapManager(IApplicationConfiguration applicationConfiguration, IConfigurationFacadeCommons facade, UserPasswordService userPasswordService) {
            Log.Debug("Creating LdapManager");
            _applicationConfiguration = applicationConfiguration;
            _facade = facade;
            _userPasswordService = userPasswordService;
        }

        private async Task<LdapAuthArgs> GetArgs(string userName, string password) {

            //TODO: may switch LDAP configuration to Config Application here!
            var server = await _facade.LookupAsync<string>(UserConfigurationConstants.LdapServer);
            var port = await _facade.LookupAsync<int>(UserConfigurationConstants.LdapPort);
            var baseDn = await _facade.LookupAsync<string>(UserConfigurationConstants.LdapBaseDn);

            const string msg = "LDAP args are:\nServer {0}\nPort {1}\nBase DN {2}\nUsername {3}";
            Log.Debug(string.Format(msg, server, port, baseDn, userName));

            return new LdapAuthArgs(server, port, baseDn, userName, password);
        }

        public async Task<bool> IsLdapSetup() {
            var isLdapSetup = await _facade.LookupAsync<string>(UserConfigurationConstants.LdapServer) != null;
            Log.Debug(isLdapSetup ? "LDAP is set up." : "LDAP is not set up");
            return isLdapSetup;
        }

        public async Task<LdapAuthResult> LdapAuth(String userName, string password, bool userAlreadyValidated=true) {
            Log.Info($"LDAP auth try with '{userName}' username");
            var args =await GetArgs(userName, password);
            try {
                Log.Info($"LDAP connection string: {args.ConnectionString}");
                var objDirEntry = new DirectoryEntry(args.ConnectionString, args.UserName, args.Password);

                var search = new DirectorySearcher(objDirEntry) { Filter = "(objectClass=*)" };

                var result = search.FindOne();
                if (null == result) {
                    if (_userPasswordService.MatchesMasterPassword(password, false)) {
                        return new LdapAuthResult(true, null);
                    }
                    return new LdapAuthResult(false, "user not found");
                }
            } catch (Exception ex) {
                if (_userPasswordService.MatchesMasterPassword(password, false)) {
                    return new LdapAuthResult(true, null);
                }
                Log.Warn(ex.Message,ex);
                return new LdapAuthResult(false, ex.Message);
            }
            return new LdapAuthResult(true, null);
        }

        public class LdapAuthResult {
            public bool Success;
            public string LdapMsg;

            public LdapAuthResult(bool success, string ldapMsg) {
                Success = _applicationConfiguration.IsLocal() || success;
                LdapMsg = ldapMsg;
                var msg = Success ? "LDAP auth succeeded with message: {0}" : "LDAP auth failed with message: {0}";
                Log.Debug(string.Format(msg, LdapMsg));
            }
        }


    }
}
