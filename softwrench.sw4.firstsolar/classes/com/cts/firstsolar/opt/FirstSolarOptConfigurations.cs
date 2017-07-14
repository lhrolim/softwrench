using System.ComponentModel.Composition;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {



    public class FirstSolarOptConfigurations : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string DefaultFromEmailKey = "/FirstSolar/OPT/DefaultFromEmail";

        public const string DefaultMeToEmailKey = "/FirstSolar/OPT/ME/DefaultToEmail";

        public const string DefaultOptInterOutageToEmailKey = "/FirstSolar/OPT/DefaultInterOutageToEmail";

        public const string DefaultDailyOutageMeetingToEmailKey = "/FirstSolar/OPT/DefaultDailyOutageMeetingToEmail";

        [Import]
        public IConfigurationFacade ConfigurationFacade { get; set; }

        [Import]
        public IApplicationConfiguration ApplicationConfiguration { get; set; }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            ConfigurationFacade.Register(DefaultFromEmailKey, new PropertyDefinition {
                Description = "Default email to be used as 'from' on the opt emails.",
                StringValue = "softwrench@firstsolar.com",
                PropertyDataType = PropertyDataType.STRING,
            });

            

            ConfigurationFacade.Register(DefaultMeToEmailKey, new PropertyDefinition {
                Description = "Default email to be used as 'to' on the maintenance engineering emails.",
                StringValue = ApplicationConfiguration.IsProd() ? "omengineering@firstsolar.com" : ApplicationConfiguration.IsUat() ? "softwrench@firstsolar.com" : "DevTeam@controltechnologysolutions.com",
                PropertyDataType = PropertyDataType.STRING,
            });

            ConfigurationFacade.Register(DefaultOptInterOutageToEmailKey, new PropertyDefinition {
                Description = "Default email to be used as 'to' on Intercompany Outage checkbox scenario.",
                StringValue = ApplicationConfiguration.IsProd() ? "omengineering@firstsolar.com" : ApplicationConfiguration.IsUat()? "softwrench@firstsolar.com" : "DevTeam@controltechnologysolutions.com",
                PropertyDataType = PropertyDataType.STRING,
            });

            ConfigurationFacade.Register(DefaultDailyOutageMeetingToEmailKey, new PropertyDefinition {
                Description = "Default emails to be used as 'to' on Daily Outage Meeting emails.",
                StringValue = "",
                PropertyDataType = PropertyDataType.STRING,
            });
        }
    }
}
