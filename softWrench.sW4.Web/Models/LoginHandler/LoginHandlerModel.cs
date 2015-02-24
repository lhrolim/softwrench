using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Util;
using System.Configuration;

namespace softWrench.sW4.Web.Models.LoginHandler {
    public class LoginHandlerModel {
        private readonly bool _isLoginEnabled;
        private readonly string _loginMessage;
        private readonly bool _incorrectLogin;
        private readonly bool _isHapagClient;
        public bool Inactivity { get; set; }
        public bool ClosePage { get; set; }

        public LoginHandlerModel(bool isLoginEnabled, string loginMessage) {
            _isLoginEnabled = isLoginEnabled;
            _loginMessage = loginMessage;
        }

        public LoginHandlerModel(bool isLoginEnabled, bool incorrectLogin, string loginMessage, bool isHapagClient) {
            _isLoginEnabled = isLoginEnabled;
            _incorrectLogin = incorrectLogin;
            _loginMessage = loginMessage;
            _isHapagClient = isHapagClient;
        }

        public LoginHandlerModel(bool isLoginEnabled, bool isHapagClient) {
            _isLoginEnabled = isLoginEnabled;
            _isHapagClient = isHapagClient;
        }

        public bool IsLoginEnabled {
            get { return _isLoginEnabled; }
        }

        public string LoginMessage {
            get { return _loginMessage; }
        }

        public bool IncorrectLogin {
            get { return _incorrectLogin; }
        }

        public bool IsHapagClient {
            get { return _isHapagClient; }
        }

        public string Version {
            get { return ApplicationConfiguration.SystemVersion; }
        }
        public string Revision {
            get { return ApplicationConfiguration.SystemRevision; }
        }
        public bool ShowRevision {
            get {
                var showRevision = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["showRevision"], out showRevision);
                return showRevision;
            }
        }
        public override string ToString() {
            return string.Format("IsLoginEnabled: {0}, LoginMessage: {1}, IncorrectLogin: {2}, Version: {3}, Revision: {4}", IsLoginEnabled, LoginMessage, IncorrectLogin, Version, Revision);
        }
    }
}