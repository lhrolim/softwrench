using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {

    public class FirstSolarWorkPackageCreationEmailHandler : FirstSolarBaseEmailService<WorkPackage> {

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        [Import]
        public FirstSolarCustomGlobalFedService GFedService { get; set; }

        public FirstSolarWorkPackageCreationEmailHandler(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade)
            : base(emailService, redirectService, appConfig, configurationFacade) {
        }

        [Transactional(DBType.Swdb)]
        public override async Task<WorkPackage> SendEmail(WorkPackage wp, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var subject = "[{0}] A new Work Package has been created".Fmt(wp.Wpnum);

            WorkPackageEmailStatus emailStatus = null;
            EmailData emailData = null;

            //if (wp.InterConnectDocs != null && !"na".EqualsIc(wp.InterConnectDocs)) {
            //    HandleInterConnectedEmail(wp, subject, out emailStatus, out emailData);
            //}

            // at first always send the email - InterConnectDocs condition will be added later
            HandleDefaultEmailCreation(wp, subject, out emailStatus, out emailData);

            if (emailStatus != null) {
                //sending sync so that we can catch and handle the exception
                EmailService.SendEmail(emailData);
                emailStatus.WorkPackage = wp;
                wp.EmailStatuses.Add(await Dao.SaveAsync(emailStatus));
            }
            return wp;
        }

        public void HandleDefaultEmailCreation(WorkPackage wp, string subject, out WorkPackageEmailStatus emailStatus, out EmailData emailData, List<EmailAttachment> attachs = null) {
            var toEmail = ConfigFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultOptInterOutageToEmailKey);

            // config not set and is not prod do not send email
            if (string.IsNullOrEmpty(toEmail) && !ApplicationConfiguration.IsProd()) {
                emailStatus = null;
                emailData = null;
                return;
            }


            // 'to' not set on config on prod load the emails from gfed
            if (string.IsNullOrEmpty(toEmail)) {
                toEmail = AsyncHelper.RunSync(() => GFedService.BuildToFromGfed(wp));
            }

            var user = SecurityFacade.CurrentUser();

            if (!string.IsNullOrEmpty(user.Email)) {
                toEmail += "; " + user.Email;
            }

            // user has no email set
            if (string.IsNullOrEmpty(toEmail)) {
                emailStatus = null;
                emailData = null;
                return;
            }

            var isProdOrUat = ApplicationConfiguration.IsUat() || ApplicationConfiguration.IsProd();
            var ccEmail = (string)null;
            if (isProdOrUat) {
                ccEmail = "brent.galyon@firstsolar.com";
                if (ApplicationConfiguration.IsProd() && "tier1".Equals(wp.Tier)) {
                    ccEmail = FirstSolarConstants.TierOneCcEmails + ", " + ccEmail;
                }
            }

            emailStatus = new WorkPackageEmailStatus {
                Email = toEmail,
                Operation = OperationConstants.CRUD_CREATE,
                Qualifier = "standard",
              	Cc = ccEmail,
                SendDate = DateTime.Now
            };

            var msg = GenerateEmailBody(wp, emailStatus);
            emailData = new EmailData(GetFrom(), toEmail, subject, msg, attachs) { Cc = ccEmail, BCc = "support@controltechnologysolutions.com" };
        }




        private string GenerateEmailBody(WorkPackage package, WorkPackageEmailStatus emailStatus) {

            BuildTemplate();

            emailStatus.WorkPackage = package;
            emailStatus = Dao.Save(emailStatus);

//            var workpackageurl = RedirectService.GetActionUrl("FirstSolarWpGenericEmail", "Ack", "token={0}&emailStatusId={1}".Fmt(package.AccessToken, emailStatus.Id));
            var workpackageurl = SafePlaceholder(RedirectService.GetApplicationUrlRoute("_workpackage", package.Id.Value));
            var wpnum = package.Wpnum;


            var msg = Template.Render(
                Hash.FromAnonymousObject(new {
                    headerurl = GetHeaderURL(),
                    workpackageurl,
                    wpnum
                }));
            return msg;
        }


        protected override string GetTemplatePath() {
            return "//Content//Customers//firstsolar//htmls//templates//workpackageemail.html";
        }

        public override string RequestI18N() {
            return "Work Package";
        }

        protected override EmailData BuildEmailData(WorkPackage request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            throw new NotImplementedException();
        }
    }
}
