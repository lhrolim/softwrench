using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;

namespace softWrench.sW4.Preferences {

    public class GlobalUserPreferencesRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IConfigurationFacade _configFacade;

        public GlobalUserPreferencesRegistry(IConfigurationFacade configFacade) {
            _configFacade = configFacade;
        }


        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _configFacade.Register(ConfigurationConstants.Filter.ApplyDefaultPreviousFilter, new PropertyDefinition {
                Description = "Whether the previous filter criteria should be applied by default or not",
                StringValue = "false",
                PropertyDataType = PropertyDataType.BOOLEAN,
                CachedOnClient = true
            });
        }
    }
}
