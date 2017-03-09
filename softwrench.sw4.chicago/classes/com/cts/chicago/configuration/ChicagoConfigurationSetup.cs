using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.config;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration {
    public class ChicagoConfigurationSetup : ISingletonComponent {
        public const string ProblemEmails = "/Chicago/IsmSync/ProblemEmails";
        public const string FormsDir = "/Chicago/Forms/FormsDir";
        public const string IbmFormsDir = "/Chicago/Forms/IbmFormsDir";

        public ChicagoConfigurationSetup(IConfigurationFacade facade) {

            facade.Register(ProblemEmails, new PropertyDefinition() {
                Description = "Comma separated list of emails receipients to send in case of synchronization failure",
                StringValue = "",
                PropertyDataType = PropertyDataType.STRING,
            });

            facade.Register(FormsDir, new PropertyDefinition() {
                Description = "Directory that contains the forms pdf files.",
                StringValue = "",
                PropertyDataType = PropertyDataType.STRING,
            });

            facade.Register(IbmFormsDir, new PropertyDefinition() {
                Description = "Directory that contains the IBM forms pdf files.",
                StringValue = "",
                PropertyDataType = PropertyDataType.STRING,
            });

            facade.Override(UserConfigurationConstants.ChangePasswordUponStart,"true");
            facade.Override(UserConfigurationConstants.MinPasswordHistorySize,"8");


        }
    }
}
