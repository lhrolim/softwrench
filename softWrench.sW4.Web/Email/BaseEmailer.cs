using System.Collections.Generic;
using System.IO;
using cts.commons.simpleinjector;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Email;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Email {
    public class BaseEmailer : ISingletonComponent {
        protected const string NoReplySendFrom = "noreply@controltechnologysolutions.com";
        protected readonly IEmailService EmailService;
        protected readonly RedirectService RedirectService;

        public BaseEmailer(IEmailService emailService, RedirectService redirectService) {
            EmailService = emailService;
            RedirectService = redirectService;
        }

        protected virtual string CreateEmailMessage(string templatePath, Hash parameters) {
            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);
            return template.Render(parameters);
        }

        protected virtual void SendEmail(string msg, BaseEmailDto dto, List<EmailAttachment> attachments) {
            var emailData = new EmailData(string.IsNullOrWhiteSpace(dto.SentBy) ? NoReplySendFrom : dto.SentBy, dto.SendTo, dto.Subject, msg, attachments);
            EmailService.SendEmail(emailData);
        }

        protected virtual Hash BaseHash(BaseEmailDto dto) {
            return Hash.FromAnonymousObject(new {
                headerurl = string.Format("{0}{1}", RedirectService.GetRootUrl(), CustomerResourceResolver.ResolveHeaderImagePath(dto.Customer)),
                customer = dto.Customer,
                username = dto.CurrentUser.UserName,
                fullname = dto.ChangedByFullName,
                ipaddress = dto.IPAddress,
                changedon = dto.ChangedOnUTC.ToString(),
                comments = dto.Comment,
                profile = ApplicationConfiguration.Profile
            });
        }
    }
}