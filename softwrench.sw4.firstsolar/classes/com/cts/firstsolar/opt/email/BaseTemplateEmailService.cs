using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Data;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public abstract class FirstSolarBaseEmailService : ISingletonComponent {

        protected const string NoReplySendFrom = "noreply@controltechnologysolutions.com";

        protected readonly IEmailService EmailService;

        protected Template Template;
        protected readonly RedirectService RedirectService;

        protected static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        private readonly string _templatePath;
        private readonly string _headerImageUrl;
        protected readonly IApplicationConfiguration AppConfig;

        public virtual async Task<IFsEmailRequest> SendEmail(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            Validate.NotNull(request, "toSend");

            var dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>();

            var msg = GenerateEmailBody(request, package, siteId);

            Log.InfoFormat("sending {0} email for {1} to {2}", RequestI18N(), request.Id, request.Email);
            var emailData = new EmailData(NoReplySendFrom, GetSendTo(request, package, siteId), GetEmailSubjectMsg(request, package, siteId), msg, attachs);
            EmailService.SendEmail(emailData);

            request.Status = RequestStatus.Sent;
            request.ActualSendTime = DateTime.Now;

            return await dao.SaveAsync(request);
        }

        public virtual string HandleSendTo(AttributeHolder data) {
            var stringOrArray = data.GetAttribute("email");
            if (stringOrArray == null) {
                return null;
            }

            if (stringOrArray is string) {
                return stringOrArray as string;
            }
            if (!(stringOrArray is Array) || ((Array)stringOrArray).Length == 0) {
                return null;
            }
            var sendToArray = ((IEnumerable)stringOrArray).Cast<object>().Select(x => x.ToString()).ToArray();
            return string.Join(", ", sendToArray);
        }

        protected FirstSolarBaseEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) {
            Log.Debug("init Log");
            EmailService = emailService;
            RedirectService = redirectService;
            AppConfig = appConfig;
            _templatePath = AppDomain.CurrentDomain.BaseDirectory + GetTemplatePath();
            _headerImageUrl = HandleHeaderImage();
        }

        protected Template BuildTemplate() {
            var templateContent = File.ReadAllText(_templatePath);
            if (Template == null || AppConfig.IsLocal()) {
                Template = Template.Parse(templateContent); // Parses and compiles the template    
            }
            return Template;
        }

        protected abstract string GetTemplatePath();

        private string HandleHeaderImage() {
            //otb image
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var clientKey = AppConfig.GetClientKey();

            if (File.Exists(baseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg")) {
                return "Content/Customers/" + clientKey + "/images/header-email.jpg";
            }
            if (File.Exists(baseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg")) {
                return "Content/Images/" + clientKey + "/header-email.jpg";
            }
            return "Content/Images/header-email.jpg";
        }

        protected string GetHeaderURL() {
            return RedirectService.GetRootUrl() + _headerImageUrl;
        }

        public abstract string GenerateEmailBody(IFsEmailRequest request, WorkPackage package, string siteId);
        protected abstract string GetEmailSubjectMsg(IFsEmailRequest request, WorkPackage package, string siteId);
        public abstract string RequestI18N();
        protected abstract string GetSendTo(IFsEmailRequest request, WorkPackage package, string siteId);

        public string FmtDate(DateTime? date) {
            if (date == null) {
                return "";
            }
            var culture = new CultureInfo("en-US");
            return date.Value.ToString("G", culture);
        }
    }
}
