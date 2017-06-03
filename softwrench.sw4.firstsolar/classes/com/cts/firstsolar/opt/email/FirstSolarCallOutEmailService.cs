using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

    public class FirstSolarCallOutEmailService : FirstSolarBaseEmailService {


        private new static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        public FirstSolarCallOutEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }

        public override string GenerateEmailBody(IFsEmailRequest request) {
            var callout = request as CallOut;

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
                    subcontractor = callout.SubContractorName == null ? "" : callout.SubContractorName,
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

        protected override string GetEmailSubjectMsg(IFsEmailRequest request) {
            var callout = request as CallOut;
            return callout == null ? "" : "[First Solar] Callout Request ({0}, {1})".Fmt(callout.ContractorStartDate, callout.SiteName);
        }

        public override string RequestI18N() {
            return "Callout";
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//calloutemail.html";
        }


    }
}
