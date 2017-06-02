using System;
using System.IO;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using Common.Logging;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email {

    public class FirstSolarMaintenanceEmailService : BaseTemplateEmailService {



        public FirstSolarMaintenanceEmailService(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) : base(emailService, redirectService, appConfig) {
            Log.Debug("init Log");
        }


        public virtual void SendCallout(MaintenanceEngineering engRequest, string email) {
            Validate.NotNull(email, "email");
            Validate.NotNull(engRequest, "engRequest");

            var msg = GenerateEmailBody(engRequest);

            var emailData = new EmailData(NoReplySendFrom, email, "[First Solar] Maintenance Engineering Request", msg);
            //TODO: Async??
            EmailService.SendEmailAsync(emailData);
        }


        public string GenerateEmailBody(MaintenanceEngineering engRequest) {
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



    }
}
