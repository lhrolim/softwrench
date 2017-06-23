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
using DotLiquid;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.api.classes.fwk.context;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {

    public class FirstSolarWorkPackageCreationEmailHandler : FirstSolarBaseEmailService<WorkPackage> {

        [Import]
        public IConfigurationFacade ConfigFacade { get; set; }

        public FirstSolarWorkPackageCreationEmailHandler(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig, IConfigurationFacade configurationFacade)
            : base(emailService, redirectService, appConfig, configurationFacade) {
        }

        [Transactional(DBType.Swdb)]
        public override async Task<WorkPackage> SendEmail(WorkPackage wp, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            var subject = "[First Solar] New WorkPackage {0} Created".Fmt(wp.Wpnum);

            WorkPackageEmailStatus emailStatus = null;
            EmailData emailData = null;

            if (!"na".EqualsIc(wp.InterConnectDocs)) {
                HandleInterConnectedEmail(wp, subject, out emailStatus, out emailData);
            }

            if (emailStatus != null) {
                //sending sync so that we can catch and handle the exception
                EmailService.SendEmail(emailData);
                emailStatus.WorkPackage = wp;
                wp.EmailStatuses.Add(await Dao.SaveAsync(emailStatus));
            }
            return wp;
        }

        public void HandleInterConnectedEmail(WorkPackage wp, string subject, out WorkPackageEmailStatus emailStatus, out EmailData emailData, List<EmailAttachment> attachs = null) {
            var toEmail = ConfigFacade.Lookup<string>(FirstSolarOptConfigurations.DefaultOptInterOutageToEmailKey);
            if (string.IsNullOrEmpty(toEmail)) {
                emailStatus = null;
                emailData = null;
                return;
            }

            emailStatus = new WorkPackageEmailStatus {
                Email = toEmail,
                Operation = OperationConstants.CRUD_CREATE,
                Qualifier = "interconnecteddocs",
                SendDate = DateTime.Now
            };

            var msg = GenerateEmailBody(wp, emailStatus);
            emailData = new EmailData(GetFrom(), toEmail, subject, msg, attachs);
        }




        private string GenerateEmailBody(WorkPackage package, WorkPackageEmailStatus emailStatus) {

            BuildTemplate();

            emailStatus.WorkPackage = package;
            emailStatus = Dao.Save(emailStatus);

            var workpackageurl = RedirectService.GetActionUrl("FirstSolarWpGenericEmail", "Ack", "token={0}&emailStatusId={1}".Fmt(package.AccessToken, emailStatus.Id));
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

        protected override EmailData BuildEmailData(IFsEmailRequest request, WorkPackage package, string siteId, List<EmailAttachment> attachs = null) {
            throw new NotImplementedException();
        }
    }
}
