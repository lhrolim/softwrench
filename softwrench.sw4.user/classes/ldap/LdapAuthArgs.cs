using System;

namespace softWrench.sW4.AUTH {
    public class LdapAuthArgs {

        private readonly string _server;
        private readonly int _portNumber;
        private readonly string _baseDn;
        private readonly string _userName;
        private readonly string _password;

        public LdapAuthArgs(string server, int portNumber, string baseDn, string userName, string password) {
            _server = server;
            _portNumber = portNumber;
            _baseDn = baseDn;
            _userName = userName;
            _password = password;
        }

        public string UserName {
            get { return _userName; }
        }

        public string Password {
            get { return _password; }
        }

        public string ConnectionString {
            get { return String.Format("LDAP://{0}:{1}/{2}", _server, _portNumber, _baseDn); }
        }
    }
}
