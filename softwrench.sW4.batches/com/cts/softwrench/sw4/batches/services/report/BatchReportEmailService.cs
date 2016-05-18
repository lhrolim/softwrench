using System;
using System.IO;
using DotLiquid;
using log4net;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.SPF;
using Hash = DotLiquid.Hash;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report {
    public class BatchReportEmailService : ISingletonComponent {

        private readonly EmailService _emailService;
        private readonly IApplicationConfiguration _appConfig;

        private Template _template;
        private readonly RedirectService _redirectService;
        private string _headerImageUrl;

        private static readonly ILog Log = LogManager.GetLogger(typeof(BatchReportEmailService));

        public BatchReportEmailService(EmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) {
            _emailService = emailService;
            _redirectService = redirectService;
            _appConfig = appConfig;
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//batches//emailreport.html";
            var templateContent = File.ReadAllText(templatePath);

            HandleHeaderImage();
        }

        private void HandleHeaderImage()
        {
            //otb image
            _headerImageUrl = "Content/Images/header-email.jpg";

            var clientKey = _appConfig.GetClientKey();
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(baseDirectory + "//Content//Customers//" + clientKey + "//images//header-email.jpg"))
            {
                _headerImageUrl = "Content/Customers/" + clientKey + "/images/header-email.jpg";
            }
            else if (File.Exists(baseDirectory + "//Content//Images//" + clientKey + "//header-email.jpg"))
            {
                _headerImageUrl = "Content/Images/" + clientKey + "/header-email.jpg";
            }
        }

        public void SendEmail(BatchReport report) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//batches//emailreport.html";
            var templateContent = File.ReadAllText(templatePath);
            _template = Template.Parse(templateContent);  // Parses and compiles the template
            var user = SecurityFacade.CurrentUser();
            if (user.Email == null) {
                Log.WarnFormat("unable to send report email, as user {0} has no email registered", user.Login);
                return;
            }
            var appurl = _redirectService.GetApplicationUrl("_batchreport", "detail", "output", report.OriginalMultiItemBatch.Id.ToString());
            var msg =
                _template.Render(
                    Hash.FromAnonymousObject(
                        new {
                            headerurl = _redirectService.GetRootUrl() + _headerImageUrl,
                            name = user.FullName,
                            sentitems = report.NumberOfSentItens,
                            problematicitems = report.NumberOfProblemItens,
                            url = appurl
                        }));

            var emailData = new EmailData("noreply@controltechnologysolutions.com", user.Email, "Batch Submission Finished", msg);
            _emailService.SendEmail(emailData);
        }
    }
}
