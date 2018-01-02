﻿using System.ComponentModel.Composition;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config {



    public class FirstSolarDispatchConfigurations : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        public const string DefaultFromEmailKey = "/FirstSolarDispatch/DefaultFromEmail";
        public const string BbcEmailsToNotify = "/FirstSolarDispatch/BccEmailsToNotify";
        public const string SmsEmailsToNotify = "/FirstSolarDispatch/SmsEmailsToNotify";

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

            ConfigurationFacade.Register(BbcEmailsToNotify, new PropertyDefinition {
                Description = "Email addresses to be used as 'bbc' on the dispatch emails.",
                StringValue = "brent.galyon@firstsolar.com",
                PropertyDataType = PropertyDataType.STRING
            });

            ConfigurationFacade.Register(SmsEmailsToNotify, new PropertyDefinition {
                Description = "Email addresses to be used as 'bbc' on the dispatch sms.",
                StringValue = "4802254926@txt.att.net", 
                PropertyDataType = PropertyDataType.STRING
            });
        }
    }
}
