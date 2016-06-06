using System;
using System.Collections.Generic;
using System.Text;
using softwrench.sw4.api.classes.email;
using Common.Logging;
using System.IO;
using DotLiquid;
using softwrench.sw4.user.classes.entities;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Web.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Email {

    /// <summary>
    /// Email metadata files.
    /// </summary>
    public class MetadataEmailer : ISingletonComponent {
        private const string NoReplySendFrom = "noreply@controltechnologysolutions.com";
        private readonly IEmailService emailService;
        private readonly IApplicationConfiguration appConfig;
        private readonly RedirectService redirectService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataEmailer));

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataEmailer"/> class.
        /// </summary>
        /// <param name="emailService">The email service reference</param>
        public MetadataEmailer(IEmailService emailService, RedirectService redirectService, IApplicationConfiguration appConfig) {
            this.emailService = emailService;
            this.appConfig = appConfig;
            this.redirectService = redirectService;
        }

        /// <summary>
        /// Email the metadata file that has been changed.
        /// </summary>        
        public void SendMetadataChangeEmail(string metadataName,
            string newMetadatacontent,
            string oldMetadataContent,
            string comments,
            User user,
            DateTime changedOn,
            string to,
            string from = NoReplySendFrom) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//metadata//metadatachangereport.html";
            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);

            var clientKey = appConfig.GetClientKey();

            var msg = template.Render(
                   Hash.FromAnonymousObject(new {
                       headerurl = string.Format("{0}{1}", redirectService.GetRootUrl(), CustomerResourceResolver.ResolveHeaderImagePath(clientKey)),
                       customer = clientKey,
                       username = user.UserName,
                       fullname = user.FullName,
                       changedon = changedOn.ToString(),
                       comments = comments,
                       filename = metadataName,
                       profile = ApplicationConfiguration.Profile
                   }));

            var attachemnts = new List<EmailAttachment>();
            attachemnts.Add(this.ConvertToMetadataAttachment(oldMetadataContent, string.Format("old_{0}", metadataName)));
            attachemnts.Add(this.ConvertToMetadataAttachment(newMetadatacontent, string.Format("new_{0}", metadataName)));

            var subject = string.Format("[softWrench {0} - {1}] Metadata Changed", clientKey, ApplicationConfiguration.Profile);

            var emailData = new EmailData(from, to, subject, msg, attachemnts);
            emailService.SendEmail(emailData);
        }

        private EmailAttachment ConvertToMetadataAttachment(string fileContent, string metadataName) {
            try {
                return new EmailAttachment() { AttachmentBinary = Encoding.UTF8.GetBytes(fileContent), AttachmentName = metadataName };
            } catch (Exception e) {
                Log.Error("error creating attachment", e);
                throw;
            } finally {
            }
        }
    }
}
