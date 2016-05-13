using System;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.UserSetup {
    public class UserSetupError : IBaseLayoutModel {
        public UserSetupError(Exception exception) {
            Exception = exception;
        }

        public string Title {
            get { return "User Setup Error"; }
        }

        public string ClientName {
            get { return ApplicationConfiguration.ClientName; }
            set { }
        }

        public Exception Exception { get; private set; }
    }
}