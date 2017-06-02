using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarCallOutEmailService : BaseTemplateEmailService {


        private new static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        public FirstSolarCallOutEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }


        public virtual async Task<CallOut> SendCallout(CallOut callout, string email, List<EmailAttachment> attachs = null) {
            Validate.NotNull(email, "email");
            Validate.NotNull(callout, "callout");


            var msg = GenerateEmailBody(callout);
            if (callout.GenerateToken()) {
                callout = await DAO.SaveAsync(callout);
            }

            Log.InfoFormat("sending callout email for {0} to {1}", callout.Id, email);
            var emailData = new EmailData(NoReplySendFrom, email, "[First Solar] Callout Request ({0}, {1})".Fmt(callout.SendTime, callout.SiteName), msg, attachs);
            EmailService.SendEmail(emailData);

            callout.Status = RequestStatus.Sent;
            callout.ActualSendTime = DateTime.Now;

            return await DAO.SaveAsync(callout);
        }


        public string GenerateEmailBody(CallOut callout) {
            BuildTemplate();

            var acceptUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionCallOut", "token={0}&status=approved".Fmt(callout.Token));
            var rejectUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionCallOut", "token={0}&status=rejected".Fmt(callout.Token));
            var pendingUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionCallOut", "token={0}&status=pending".Fmt(callout.Token));

            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    headerurl = GetHeaderURL(),
                    acceptUrl,
                    rejectUrl,
                    pendingUrl,
                    subcontractor = callout.SubContractor == null ? "" : callout.SubContractor.Name,
                    expirationdate = callout.ExpirationDate?.ToString("yyyy'-'MM'-'dd HH':'mm':'ss") ?? "",
                    contractorstartdate= callout.ContractorStartDate?.ToString("yyyy'-'MM'-'dd HH':'mm':'ss") ?? "",
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
