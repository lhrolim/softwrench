using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using DotLiquid;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Setup {

    public class UserSetupEmailService : ISingletonComponent {

        private readonly EmailService _emailService;

        private Template _template;
        private readonly RedirectService _redirectService;
        private readonly IUserLinkManager _linkManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserSetupEmailService));

        private readonly string _automaticTemplatePath;
        private readonly string _manualTemplatePath;

        public UserSetupEmailService(EmailService emailService, RedirectService redirectService, IUserLinkManager linkManager) {
            Log.Debug("init Log");
            _emailService = emailService;
            _redirectService = redirectService;
            _linkManager = linkManager;
            _automaticTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_automaticpassword.html";
            _manualTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_manualpassword.html";
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

            var emailData = new EmailService.EmailData("noreply@controltechnologysolutions.com", email, "Welcome to softwrench", msg);
            _emailService.SendEmail(emailData);

        }

    }
}
