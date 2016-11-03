using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.configuration {
    public class KogtGlobalPreferenceRegistry : ISWEventListener<ApplicationStartedEvent> {

        private readonly IConfigurationFacade _facade;

        public KogtGlobalPreferenceRegistry(IConfigurationFacade facade) {
            _facade = facade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _facade.Override(ConfigurationConstants.Filter.ApplyDefaultPreviousFilter,"true");
        }
    }
}
