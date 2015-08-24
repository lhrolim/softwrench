using System;
using System.IO;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.user.classes.services.setup {

    public class UserSetupEmailService : ISingletonComponent {

        private readonly IEmailService _emailService;

        private Template _template;
        private readonly RedirectService _redirectService;
        private readonly UserLinkManager _linkManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserSetupEmailService));

        private readonly string _automaticTemplatePath;
        private readonly string _manualTemplatePath;
        private string _forgotPasswordTemplatePath;

        public UserSetupEmailService(IEmailService emailService, RedirectService redirectService, UserLinkManager linkManager) {
            Log.Debug("init Log");
            _emailService = emailService;
            _redirectService = redirectService;
            _linkManager = linkManager;
            _automaticTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_automaticpassword.html";
            _manualTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_manualpassword.html";
            _forgotPasswordTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//forgotpassword.html";
        }

        public void SendEmail(User user, string email) {
            Validate.NotNull(email, "email");
            Validate.NotNull(user, "user");
            var automaticMode = user.Password == null;
            string templateToUse;
            string linkUrl;
            if (automaticMode) {
                templateToUse = _automaticTemplatePath;
                var tokenLink = _linkManager.GenerateTokenLink(user);
                linkUrl = _redirectService.GetActionUrl("UserSetup", "DefinePassword", "tokenLink={0}".Fmt(tokenLink));
            } else {
                templateToUse = _manualTemplatePath;
                linkUrl = _redirectService.GetRootUrl();
            }

            var templateContent = File.ReadAllText(templateToUse);
            _template = Template.Parse(templateContent);  // Parses and compiles the template


            var msg = _template.Render(
                    Hash.FromAnonymousObject(new {
                        name = user.FullName,
                        link = linkUrl,
                        //password won´t be used for automatic template, but let´s put it here anyway
                        password = user.Password
                    }));

            var emailData = new EmailData("noreply@controltechnologysolutions.com", email, "[Softwrench]Welcome to softwrench", msg);
            _emailService.SendEmail(emailData);

        }

        public void ForgotPasswordEmail(User user, string email)
        {
            Validate.NotNull(email, "email");
            Validate.NotNull(user, "user");
            var templateToUse = _forgotPasswordTemplatePath;
            var tokenLink = _linkManager.GenerateTokenLink(user);
            var linkUrl = _redirectService.GetActionUrl("UserSetup", "DefinePassword", "tokenLink={0}".Fmt(tokenLink));

            var templateContent = File.ReadAllText(templateToUse);
            _template = Template.Parse(templateContent);  // Parses and compiles the template

            var msg = _template.Render(
                    Hash.FromAnonymousObject(new {
                        name = user.FullName,
                        link = linkUrl,
                        //password won´t be used for automatic template, but let´s put it here anyway
                        password = user.Password
                    }));

            var emailData = new EmailData("noreply@controltechnologysolutions.com", email, "[Softwrench]Change Password Instructions", msg);
            _emailService.SendEmail(emailData);

        }
    }
}
