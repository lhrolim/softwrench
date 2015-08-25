using System;
using System.IO;
using System.Net.Cache;
using System.Security.Policy;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.user.classes.entities;
using Hash = DotLiquid.Hash;

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
        private string _headerImageUrl;

        public UserSetupEmailService(IEmailService emailService, RedirectService redirectService, UserLinkManager linkManager, IApplicationConfiguration appConfig) {
            Log.Debug("init Log");
            _emailService = emailService;
            _redirectService = redirectService;
            _linkManager = linkManager;
            _automaticTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_automaticpassword.html";
            _manualTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_manualpassword.html";
            _forgotPasswordTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//forgotpassword.html";

            var clientKey = appConfig.GetClientKey();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg"))
            {
                _headerImageUrl = "Content/Customers/" + clientKey + "/images/header-email.jpg";
            } else if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg"))
            {
                _headerImageUrl = "Content/Images/" + clientKey + "/header-email.jpg";
            } else
            {
                _headerImageUrl = "Content/Images/header-email.jpg";
            }
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
                        headerurl = _redirectService.GetRootUrl() + _headerImageUrl,
                        name = user.FullName,
                        link = linkUrl,
                        //password won´t be used for automatic template, but let´s put it here anyway
                        password = user.Password
                    }));

            var emailData = new EmailData("noreply@controltechnologysolutions.com", email, "[softWrench] Welcome to softWrench", msg);
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

            //var test = _redirectService.GetRootUrl();

            var msg = _template.Render(
                    Hash.FromAnonymousObject(new {
                        headerurl = _redirectService.GetRootUrl() + _headerImageUrl,
                        name = user.FullName,
                        link = linkUrl,
                        //password won´t be used for automatic template, but let´s put it here anyway
                        password = user.Password
                    }));

            var emailData = new EmailData("noreply@controltechnologysolutions.com", email, "[softWrench] Change Password Instructions", msg);
            _emailService.SendEmail(emailData);

        }
    }
}
