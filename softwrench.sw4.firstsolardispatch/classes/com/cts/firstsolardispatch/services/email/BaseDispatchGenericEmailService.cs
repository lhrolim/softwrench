using System;
using System.Globalization;
using System.IO;
using cts.commons.simpleinjector;
using DotLiquid;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public abstract class BaseDispatchGenericEmailService : ISingletonComponent {

        protected Template Template;

        protected void BuildTemplate() {
            if (Template != null && !ApplicationConfiguration.IsLocal()) return;
            var templateContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + GetTemplateFilePath());
            Template = Template.Parse(templateContent); // Parses and compiles the template  
        }

     
        protected string GetFrom() {
            var configFacade = SimpleInjectorGenericFactory.Instance.GetObject<ConfigurationFacade>();
            return configFacade.Lookup<string>(FirstSolarDispatchConfigurations.DefaultFromEmailKey);
        }

        protected abstract string GetTemplateFilePath();

        public class DispatchOptions {
            public bool SendSms { get; set; }
            public int Hour { get; set; }

            public DispatchOptions(bool sendSms, int hour) {
                SendSms = sendSms;
                Hour = hour;
            }
        }
    }
}
