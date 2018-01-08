using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using cts.commons.simpleinjector;
using DotLiquid;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.config;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public abstract class BaseDispatchGenericEmailService : ISingletonComponent {

        protected Template Template;

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        protected void BuildTemplate() {
            if (Template != null && !ApplicationConfiguration.IsLocal()) return;
            var templateContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + GetTemplateFilePath());
            Template = Template.Parse(templateContent); // Parses and compiles the template  
        }

     
        protected string GetFrom() {
            return ConfigFacade.Lookup<string>(FirstSolarDispatchConfigurations.DefaultFromEmailKey);
        }

        protected abstract string GetTemplateFilePath();

        protected string BuildMessage(object templateData) {
            BuildTemplate();
            return Template.Render(Hash.FromAnonymousObject(templateData));
        }

        protected virtual string GetBcc() {
            var bbc = SwConstants.DevTeamEmail + "; ";
            bbc += ConfigFacade.Lookup<string>(FirstSolarDispatchConfigurations.BccEmailsToNotify);

            return bbc;
        }

        
    }
}
