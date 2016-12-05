using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.offlineserver.services.util {
    public class OffLineConfigurationRegister : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IConfigurationFacade _configFacade;

        public OffLineConfigurationRegister(IConfigurationFacade configFacade) {
            _configFacade = configFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _configFacade.Register(OfflineConstants.AsyncBatchMinSize, new PropertyDefinition {
                Description = "minimum size of the batch so that it executes asynchronously",
                StringValue = "2",
                PropertyDataType = PropertyDataType.LONG,
            });

            _configFacade.Register(OfflineConstants.SupportContactEmail, new PropertyDefinition() {
                Description = "Support email the offline app should contact",
                PropertyDataType = PropertyDataType.STRING,
                StringValue = "support@controltechnologysolutions.com",
                DefaultValue = "support@controltechnologysolutions.com"
            });
        }
    }
}
