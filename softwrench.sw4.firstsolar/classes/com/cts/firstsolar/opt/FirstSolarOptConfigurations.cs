using System.ComponentModel.Composition;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {



    public class FirstSolarOptConfigurations : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string DefaultFromEmailKey = "/FirstSolar/OPT/DefaultFromEmail";

        [Import]
        public IConfigurationFacade ConfigurationFacade { get; set; }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            ConfigurationFacade.Register(DefaultFromEmailKey, new PropertyDefinition {
                Description = "Default email to be used as 'from' on the opt emails.",
                StringValue = "softwrench@firstsolar.com",
                PropertyDataType = PropertyDataType.STRING,
            });
        }
    }
}
