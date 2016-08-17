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
        private readonly RedirectService redirectService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataEmailer));

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataEmailer"/> class.
        /// </summary>
        /// <param name="emailService">The email service reference</param>
        public MetadataEmailer(IEmailService emailService, RedirectService redirectService) {
            this.emailService = emailService;            
            this.redirectService = redirectService;
        }

        /// <summary>
        /// Email the metadata file that has been changed.
        /// </summary>        
        public void SendMetadataChangeEmail(MetadataChangeEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//metadata//metadatachangereport.html";
            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);
            var msg = template.Render(
                   Hash.FromAnonymousObject(new {
                       headerurl = string.Format("{0}{1}", redirectService.GetRootUrl(), CustomerResourceResolver.ResolveHeaderImagePath(email.Customer)),
                       customer = email.Customer,
                       username = email.CurrentUser.UserName,
                       fullname = email.ChangedByFullName,
                       ipaddress = email.IPAddress,
                       changedon = email.ChangedOnUTC.ToString(),
                       comments = email.Comment,
                       filename = email.MetadataName,
                       profile = ApplicationConfiguration.Profile
                   }));

            var attachemnts = new List<EmailAttachment>();
            attachemnts.Add(this.ConvertToMetadataAttachment(email.OldFileContent, string.Format("old_{0}", email.MetadataName)));
            attachemnts.Add(this.ConvertToMetadataAttachment(email.NewFileContent, string.Format("new_{0}", email.MetadataName)));

            var emailData = new EmailData(string.IsNullOrWhiteSpace(email.SentBy) ? NoReplySendFrom : email.SentBy, email.SendTo, email.Subject, msg, attachemnts);
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

    public class MetadataChangeEmail {
        public string Customer { get; set; }
        public User CurrentUser { get; set; }
        public string ChangedByFullName { get; set; }
        public DateTime ChangedOnUTC { get; set; }
        public string IPAddress { get; set; }
        public string MetadataName { get; set; }
        public string NewFileContent { get; set; }
        public string OldFileContent { get; set; }
        public string Comment { get; set; }       
        public string SendTo { get; set; }
        public string SentBy { get; set; }
        public string Subject { get; set; }
    }

}
