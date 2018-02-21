using System;
using softwrench.sw4.webcommons.classes.api;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.UserSetup {
    public class UserSetupError : ABaseLayoutModel {
        public UserSetupError(Exception exception) {
            Exception = exception;
        }

        public override string Title => "User Setup Error";

        public override string ClientName {
            get { return ApplicationConfiguration.ClientName; }
            set { }
        }

        public Exception Exception { get; private set; }
    }
}