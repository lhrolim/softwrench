using System;
using System.DirectoryServices;
using Iesi.Collections.Generic;
using softWrench.sW4.Configuration;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Util;
using IComponent = softWrench.sW4.SimpleInjector.IComponent;

namespace softWrench.sW4.AUTH {
    public class LdapManager : IComponent {

        private IConfigurationFacade _facade;
        private readonly SWDBHibernateDAO _dao;


        public LdapManager(IConfigurationFacade facade, SWDBHibernateDAO dao) {
            _facade = facade;
            _dao = dao;

        }

        private LdapAuthArgs GetArgs(string userName, string password) {
            //TODO: may switch LDAP configuration to Config Application here!
            return new LdapAuthArgs(
                   ApplicationConfiguration.LdapServer,
                   ApplicationConfiguration.LdapPortNumber,
                   ApplicationConfiguration.LdapBaseDn,
                   userName,
                   password
                   );
        }

        public Boolean IsLdapSetup() {
            return ApplicationConfiguration.LdapServer != null;
        }

        public LdapAuthResult LdapAuth(String userName, string password) {
            var args = GetArgs(userName, password);
            try {
                var objDirEntry = new DirectoryEntry(args.ConnectionString, args.UserName, args.Password);

                var search = new DirectorySearcher(objDirEntry) { Filter = "(objectClass=*)" };

                var result = search.FindOne();
                if (null == result) {
                    return new LdapAuthResult(false, "user not found");
                }
            } catch (Exception ex) {
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
            }
        }


    }
}
