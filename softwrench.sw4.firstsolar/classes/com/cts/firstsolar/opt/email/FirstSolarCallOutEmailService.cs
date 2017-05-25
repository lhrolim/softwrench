using System.IO;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarCallOutEmailService : BaseTemplateEmailService {

        
        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        public FirstSolarCallOutEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }


        public virtual void SendCallout(CallOut callout, string email) {
            Validate.NotNull(email, "email");
            Validate.NotNull(callout, "callout");

            var msg = GenerateEmailBody(callout);

            Log.InfoFormat("sending callout email for {0} to {1}", callout.Id, email);
            var emailData = new EmailData(NoReplySendFrom, email, "[softWrench] Call Out Addressed", msg);
            EmailService.SendEmailAsync(emailData);
        }


        public string GenerateEmailBody(CallOut callout) {
            BuildTemplate();

            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    headerurl = GetHeaderURL(),
                    subcontractor = callout.SubContractor == null ? "" : callout.SubContractor.Name,
                    expirationdate = callout.ExpirationDate?.ToString("yyyy'-'MM'-'dd HH':'mm':'ss") ?? "",
                    ponumber = callout.PoNumber,
                    tonumber = callout.ToNumber,
                    site = callout.SiteName,
                    billing = callout.BillingEntity,
                    nottoexceed = callout.NotToExceedAmount,
                    remaining = callout.RemainingFunds,
                    scopeofwork = callout.ScopeOfWork,
                    fsplant = callout.PlantContacts,
                    otherinfo = callout.OtherInfo
                }));
            return msg;
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//calloutemail.html";
        }


    }
}
