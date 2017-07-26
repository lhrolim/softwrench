using System.Collections.Generic;
using System.ComponentModel.Composition;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.app;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarCallOutEmailService : FirstSolarBaseEmailRequestEmailService {


        private new static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarCallOutEmailService));

        [Import]
        public SWDBHibernateDAO DAO { get; set; }

        public FirstSolarCallOutEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade) : base(emailService, redirectService, appConfig, configurationFacade) {
            Log.Debug("init Log");
        }

        protected override EmailData BuildEmailData(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var callout = request as CallOut;
            var subject = callout == null ? "[First Solar] Callout Request" : "[First Solar] Callout Request ({0}, {1})".Fmt(FmtDate(callout.ContractorStartDate), callout.FacilityName);

            var msg = GenerateEmailBody(request, package, siteId);
            var emailData = new EmailData(GetFrom(), request.Email, subject, msg, attachs) {Cc = request.Cc};
            return emailData;
        }

        public override void HandleReject(IFsEmailRequest request, WorkPackage package) {
            // nothing done on callout reject
        }

        public override string RequestI18N() {
            return "Callout";
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//calloutemail.html";
        }

        public string GenerateEmailBody(IFsEmailRequest request, WorkPackage package, string siteId) {
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
                    expirationdate = FmtDate(callout.ExpirationDate),
                    contractorstartdate = FmtDate(callout.ContractorStartDate),
                    ponumber = callout.PoNumber,
                    tonumber = callout.ToNumber,
                    site = callout.FacilityName,
                    address = callout.FacilityAddress,
                    city = callout.FacilityCity,
                    state = callout.FacilityState,
                    zip = callout.FacilityPostalCode,
                    billing = callout.BillingEntity,
                    nottoexceed = callout.NotToExceedAmount,
                    remaining = callout.RemainingFunds,
                    scopeofwork = callout.ScopeOfWork,
                    fsplant = callout.PlantContacts,
                    otherinfo = callout.OtherInfo
                }));
            return msg;
        }
    }
}
