﻿using System;
using System.IO;
using System.Security.Policy;
using DotLiquid;
using log4net;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using Hash = DotLiquid.Hash;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.report2 {
    public class BatchReportEmailService : ISingletonComponent {

        private readonly EmailService _emailService;

        private readonly Template _template;
        private readonly RedirectService _redirectService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(BatchReportEmailService));



        public BatchReportEmailService(EmailService emailService, RedirectService redirectService) {
            _emailService = emailService;
            _redirectService = redirectService;
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//batches//emailreport.html";
            var templateContent = File.ReadAllText(templatePath);
            _template = Template.Parse(templateContent);  // Parses and compiles the template
        }

        public void SendEmail(BatchReport report) {
            var user = SecurityFacade.CurrentUser();
            if (user.Email == null) {
                Log.WarnFormat("unable to send report email, as user {0} has no email registered", user.Login);
                return;
            }
            var appurl = _redirectService.GetApplicationUrl("_batchreport", "detail", "output", report.OriginalBatch.Id.ToString());
            var msg =
                _template.Render(
                    Hash.FromAnonymousObject(
                        new {
                            name = user.FullName,
                            sentitems = report.NumberOfSentItens,
                            problematicitems = report.NumberOfProblemItens,
                            url = appurl
                        }));
            var emailData = new EmailService.EmailData("noreply@controltechnologysolutions.com", user.Email, "Batch Submission Finished", msg);
            _emailService.SendEmail(emailData);

        }

    }
}
