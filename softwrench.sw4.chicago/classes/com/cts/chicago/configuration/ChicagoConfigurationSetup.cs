using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.config;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration {
    public class ChicagoConfigurationSetup : ISingletonComponent {
        public const string ProblemEmails = "/Chicago/IsmSync/ProblemEmails";


        public ChicagoConfigurationSetup(IConfigurationFacade facade) {

            facade.Register(ProblemEmails, new PropertyDefinition() {
                Description = "Comma separated list of emails receipients to send in case of synchronization failure",
                StringValue = "",
                PropertyDataType = PropertyDataType.STRING,
            });

            facade.Override(UserConfigurationConstants.ChangePasswordUponStart,"true");
            facade.Override(UserConfigurationConstants.MinPasswordHistorySize,"8");


        }
    }
}
