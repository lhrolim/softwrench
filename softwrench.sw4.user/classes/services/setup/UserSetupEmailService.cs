using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.ldap;
using Hash = DotLiquid.Hash;

namespace softwrench.sw4.user.classes.services.setup {

    public class UserSetupEmailService : ISingletonComponent {

        private const string NoReplySendFrom = "noreply@controltechnologysolutions.com";

        private readonly IEmailService _emailService;

        private Template _template;
        private readonly RedirectService _redirectService;
        private readonly UserLinkManager _linkManager;
        private readonly IApplicationConfiguration _appConfig;
        private LdapManager _ldapManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserSetupEmailService));

        private readonly string _automaticTemplatePath;
        private readonly string _manualTemplatePath;
        private readonly string _ldapPasswordTemplatePath;
        private readonly string _forgotPasswordTemplatePath;
        private readonly string _newUserRegistrationApprovalRequestTemplatePath;
        private string _headerImageUrl;

        public UserSetupEmailService(IEmailService emailService, RedirectService redirectService, UserLinkManager linkManager, IApplicationConfiguration appConfig, LdapManager ldapManager) {
            Log.Debug("init Log");
            _emailService = emailService;
            _redirectService = redirectService;
            _linkManager = linkManager;
            _appConfig = appConfig;
            _ldapManager = ldapManager;
            _automaticTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_automaticpassword.html";
            _ldapPasswordTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_ldapsetup.html";
            _manualTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//welcomeemail_manualpassword.html";
            _forgotPasswordTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//forgotpassword.html";
            _newUserRegistrationApprovalRequestTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//usersetup//newuserapproval.html";
            if (_appConfig != null) {
                //test
                HandleHeaderImage();
            }

        }

        private void HandleHeaderImage() {
            //otb image
            _headerImageUrl = "Content/Images/header-email.jpg";

            var clientKey = _appConfig.GetClientKey();
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(baseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg")) {
                _headerImageUrl = "Content/Customers/" + clientKey + "/images/header-email.jpg";
            } else if (File.Exists(baseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg")) {
                _headerImageUrl = "Content/Images/" + clientKey + "/header-email.jpg";
            }
        }

        public virtual async Task SendActivationEmail(User user, string email, string openPassword = null) {
            Validate.NotNull(email, "email");
            Validate.NotNull(user, "user");
            var automaticMode = openPassword == null;
            var isLdap = await _ldapManager.IsLdapSetup();

            string templateToUse;
            string linkUrl;
            if (isLdap) {
                templateToUse = _ldapPasswordTemplatePath;
                linkUrl = _redirectService.GetRootUrl();
            } else if (automaticMode) {
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
                        password = openPassword
                    }));

            var emailData = new EmailData(NoReplySendFrom, email, "[softWrench] Welcome to softWrench", msg);
            //TODO: Async??
            _emailService.SendEmail(emailData);
        }

        public void ForgotPasswordEmail(User user, string email) {
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

            var emailData = new EmailData(NoReplySendFrom, email, "[softWrench] Change Password Instructions", msg);
            _emailService.SendEmail(emailData);
        }

        /// <summary>
        /// Will send an email to all `approverEmails` with instructions on how to activate the user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="approverEmails"></param>
        public void NewUserApprovalRequestEmail(string username, string firstname, string lastname, IEnumerable<string> approverEmails) {
            Validate.NotEmpty(username, "username");
            Validate.NotEmpty(firstname, "firstname");
            Validate.NotEmpty(lastname, "lastname");

            var approverEmailsList = approverEmails.ToList();
            Validate.NotEmpty(approverEmailsList, "approverEmails");

            var sendTo = approverEmailsList.Count == 1
                ? approverEmailsList.First()
                : approverEmailsList.Aggregate("", (concat, current) => concat + "," + current);

            var templateContent = File.ReadAllText(_newUserRegistrationApprovalRequestTemplatePath);
            var template = Template.Parse(templateContent);

            var message = template.Render(Hash.FromAnonymousObject(new {
                headerurl = _redirectService.GetRootUrl() + _headerImageUrl,
                systemurl = _redirectService.GetRootUrl(),
                firstname,
                lastname,
                username
            }));

            var emailData = new EmailData(NoReplySendFrom, sendTo, "[softWrench] User Creation Awaiting Approval", message);
            _emailService.SendEmail(emailData);
        }

        /// <summary>
        /// Sends an email with a generic message.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="fireAndForget">whether or not to send the email in a fire-and-forget way. Defaults to <code>false</code></param>
        public void GenericMessageEmail(string userEmail, string subject, string message, bool fireAndForget = false) {
            Validate.NotEmpty(userEmail, "userEmail");
            Validate.NotEmpty(subject, "subject");
            Validate.NotEmpty(message, "message");

            var mail = new EmailData(NoReplySendFrom, userEmail, subject, message);
            if (fireAndForget) {
                _emailService.SendEmailAsync(mail);
            } else {
                _emailService.SendEmail(mail);
            }
        }

    }
}
