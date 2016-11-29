using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.user.classes.config {
    public class UserConfigurationConstants {

        public const string TokenExpireDuration = "/Global/User/TokenExpireDuration";
        public const string MinPasswordHistorySize = "/Global/Password/MinPasswordHistory";
        public const string PasswordExpirationTime = "/Global/Password/ExpirationTime";
        /// <summary>
        /// Number of wrong attempts before the password gets locked
        /// </summary>
        public const string WrongPasswordAttempts = "/Global/Password/AttemptsBeforeBlock";
        public const string ChangePasswordUponStart = "/Global/Password/ChangeUponStart";

        public const string LdapServer = "/Global/Ldap/LdapServer";
        public const string LdapPort = "/Global/Ldap/LdapPort";
        public const string LdapBaseDn = "/Global/Ldap/LdapBaseDN";


    }
}
