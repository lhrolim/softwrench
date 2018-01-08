using System.ComponentModel.Composition;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config {



    public class FirstSolarDispatchConfigurations : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string DefaultFromEmailKey = "/FirstSolarDispatch/DefaultFromEmail";
        public const string BccEmailsToNotify = "/FirstSolarDispatch/BccEmailsToNotify";
        public const string BccSmsEmailsToNotify = "/FirstSolarDispatch/BccSmsEmailsToNotify";
        public const string ToSmsEmailsToNotify = "/FirstSolarDispatch/SmsEmailsToNotify";

        [Import]
        public IConfigurationFacade ConfigurationFacade { get; set; }

        [Import]
        public IApplicationConfiguration ApplicationConfiguration { get; set; }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            ConfigurationFacade.Register(DefaultFromEmailKey, new PropertyDefinition {
                Description = "Default email to be used as 'from' on the dispatch emails.",
                StringValue = "noreply@controltechnologysolutions.com",
                PropertyDataType = PropertyDataType.STRING
            });

            ConfigurationFacade.Register(BccEmailsToNotify, new PropertyDefinition {
                Description = "Email addresses to be used as 'bcc' on the dispatch emails.",
                StringValue = "brent.galyon@firstsolar.com",
                PropertyDataType = PropertyDataType.STRING
            });

            ConfigurationFacade.Register(BccSmsEmailsToNotify, new PropertyDefinition {
                Description = "Email addresses to be used as 'bcc' on the dispatch sms.",
                StringValue = ApplicationConfiguration.IsProd() ? "4802254926@txt.att.net" : null,  
                PropertyDataType = PropertyDataType.STRING
            });

            ConfigurationFacade.Register(ToSmsEmailsToNotify, new PropertyDefinition {
                Description = "Email addresses to send on the dispatch sms.",
                PropertyDataType = PropertyDataType.STRING
            });
        }
    }
}
