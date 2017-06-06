using cts.commons.portable.Util;
using cts.commons.simpleinjector.app;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarMaintenanceEmailService : FirstSolarBaseEmailService {



        public FirstSolarMaintenanceEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }

        public override string GenerateEmailBody(IFsEmailRequest request) {
            var engRequest = request as MaintenanceEngineering;

            BuildTemplate();

            var acceptUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=approved".Fmt(engRequest.Token));
            var rejectUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=rejected".Fmt(engRequest.Token));
            var pendingUrl = RedirectService.GetActionUrl("FirstSolarEmail", "TransitionMaintenanceEngineering", "token={0}&status=pending".Fmt(engRequest.Token));

            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    acceptUrl,
                    rejectUrl,
                    pendingUrl,
                    headerurl = GetHeaderURL(),
                    engineer = engRequest.Engineer,
                    reason = engRequest.Reason
                }));
            return msg;
        }

        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//maintenanceengineeremail.html";
        }

        protected override string GetEmailSubjectMsg(IFsEmailRequest request) {
            return "[First Solar] Maintenance Engineering Request";
        }

        public override string RequestI18N() {
            return "Maintenance Engineering";
        }

        protected override string GetSendTo(IFsEmailRequest request) {
            var isProdOrUat = ApplicationConfiguration.Profile.Contains("uat") || ApplicationConfiguration.Profile.Contains("prod");
            return isProdOrUat ? request.Email + ",omengineering@firstsolar.com" : request.Email;
        }
    }
}
