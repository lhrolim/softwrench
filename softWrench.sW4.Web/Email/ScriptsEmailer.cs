using System;
using System.Collections.Generic;
using cts.commons.simpleinjector.app;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Email {
    public class ScriptsEmailer : BaseEmailer, IDynComponentEmailer {
        private readonly IConfigurationFacade _configurationFacade;
        private readonly IApplicationConfiguration _appConfig;

        public ScriptsEmailer(IEmailService emailService, RedirectService redirectService, IConfigurationFacade configurationFacade, IApplicationConfiguration appConfig) : base(emailService, redirectService) {
            _configurationFacade = configurationFacade;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Email the warn that the container was reloaded.
        /// </summary>        
        public virtual void SendContainerReloadEmail(ContainerReloadEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//dynamic//containerreload.html";
            var hash = BaseHash(email);
            hash["onContainer"] = email.OnContainer;
            hash["deployed"] = email.Deployed;
            hash["undeployed"] = email.Undeployed;
            var msg = CreateEmailMessage(templatePath, hash);
            SendEmail(msg, email, new List<EmailAttachment>());
        }

        /// <summary>
        /// Email the dynamic component created.
        /// </summary>        
        public virtual void SendDynComponentCreatedEmail(DynComponentCreatedEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//dynamic//dyncomponentcreated.html";
            var hash = BaseDynComponentHash(email);
            hash["entryName"] = email.Entry.Name;
            hash["entryTarget"] = email.Entry.Target;
            hash["entryVersion"] = email.Entry.Appliestoversion;
            hash["entryDeploy"] = email.Entry.Deploy;
            var msg = CreateEmailMessage(templatePath, hash);

            var attachemnts = new List<EmailAttachment> {
                EmailService.CreateAttachment(email.Entry.Script, "script.cs")
            };
            SendEmail(msg, email, attachemnts);
        }

        /// <summary>
        /// Email the dynamic component changes.
        /// </summary>        
        public virtual void SendDynComponentUpdatedEmail(DynComponentUpdatedEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//dynamic//dyncomponentupdated.html";
            var hash = BaseDynComponentHash(email);
            hash["entryName"] = email.NewEntry.Name;
            hash["entryTarget"] = email.NewEntry.Target;
            hash["entryVersion"] = email.NewEntry.Appliestoversion;
            hash["entryDeploy"] = email.NewEntry.Deploy;

            hash["oldEntryName"] = email.OldEntry.Name;
            hash["oldEntryTarget"] = email.OldEntry.Target;
            hash["oldEntryVersion"] = email.OldEntry.Appliestoversion;
            hash["oldEntryDeploy"] = email.OldEntry.Deploy;
            var msg = CreateEmailMessage(templatePath, hash);

            var attachemnts = new List<EmailAttachment> {
                EmailService.CreateAttachment(email.OldEntry.Script, "old_script.cs"),
                EmailService.CreateAttachment(email.NewEntry.Script, "new_script.cs")
            };
            SendEmail(msg, email, attachemnts);
        }

        /// <summary>
        /// Email the dynamic component deleted.
        /// </summary>        
        public virtual void SendDynComponentDeletedEmail(DynComponentDeleteEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//dynamic//dyncomponentdeleted.html";
            var hash = BaseDynComponentHash(email);
            hash["entryName"] = email.Entry.Name;
            hash["entryTarget"] = email.Entry.Target;
            hash["entryVersion"] = email.Entry.Appliestoversion;
            hash["entryDeploy"] = email.Entry.Deploy;
            var msg = CreateEmailMessage(templatePath, hash);

            var attachemnts = new List<EmailAttachment> {
                EmailService.CreateAttachment(email.Entry.Script, "script.cs")
            };
            SendEmail(msg, email, attachemnts);
        }

        public virtual bool FillBaseEmailDTO(BaseEmailDto dto, string ipAddress, string comment, string userName, string subject) {
            var sendTo = _configurationFacade.Lookup<string>(ConfigurationConstants.MetadataChangeReportEmailId);
            if (string.IsNullOrEmpty(sendTo)) {
                return false;
            }

            dto.Customer = _appConfig.GetClientKey();
            dto.IPAddress = ipAddress;
            dto.Comment = comment;
            dto.ChangedByFullName = userName;
            dto.CurrentUser = SecurityFacade.CurrentUser().DBUser;
            dto.ChangedOnUTC = DateTime.UtcNow;
            dto.SendTo = sendTo;
            dto.Subject = GetSubject(subject);
            return true;
        }

        protected virtual string GetSubject(string sufix) {
            return string.Format("[softWrench {0} - {1}] {2}", _appConfig.GetClientKey(),
                ApplicationConfiguration.Profile, sufix);
        }

        protected virtual Hash BaseDynComponentHash(BaseDynComponentEmailDto email) {
            var hash = BaseHash(email);
            hash["reloadSufix"] = email.ReloadSufix ?? "";
            return hash;
        }
    }

    public class ContainerReloadEmail : BaseEmailDto {
        public string OnContainer { get; set; }
        public string Deployed { get; set; }
        public string Undeployed { get; set; }
    }
}
