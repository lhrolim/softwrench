using System;
using System.Collections.Generic;
using softwrench.sw4.api.classes.email;
using DotLiquid;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Email;
using softWrench.sW4.Web.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Email {

    /// <summary>
    /// Email metadata files.
    /// </summary>
    public class MetadataEmailer : BaseEmailer {

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataEmailer"/> class.
        /// </summary>
        /// <param name="emailService">The email service reference</param>
        /// <param name="redirectService"></param>
        public MetadataEmailer(IEmailService emailService, RedirectService redirectService) : base(emailService, redirectService) {
        }

        /// <summary>
        /// Email the metadata file that has been changed.
        /// </summary>        
        public void SendMetadataChangeEmail(MetadataChangeEmail email) {
            var templatePath = AppDomain.CurrentDomain.BaseDirectory + "//Content//Templates//metadata//metadatachangereport.html";
            var hash = BaseHash(email);
            hash["filename"] = email.MetadataName;
            var msg = CreateEmailMessage(templatePath, hash);

            var attachemnts = new List<EmailAttachment>();
            if (!string.IsNullOrWhiteSpace(email.OldFileContent)) {
                attachemnts.Add(EmailService.CreateAttachment(email.OldFileContent, string.Format("old_{0}", email.MetadataName)));
            }
            if (!string.IsNullOrWhiteSpace(email.NewFileContent)) {
                attachemnts.Add(EmailService.CreateAttachment(email.NewFileContent, string.Format("new_{0}", email.MetadataName)));
            }
            SendEmail(msg, email, attachemnts);
        }
    }

    public class MetadataChangeEmail : BaseEmailDto {
        public string MetadataName { get; set; }
        public string NewFileContent { get; set; }
        public string OldFileContent { get; set; }
    }
}
