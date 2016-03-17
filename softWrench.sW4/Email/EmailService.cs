using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using System.Net.Mail;
using System.Threading.Tasks;
using cts.commons.Util;
using softWrench.sW4.Metadata;
using Common.Logging;
using softwrench.sw4.api.classes;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Util;


namespace softWrench.sW4.Email {
    public class EmailService : IEmailService {

        private static readonly Regex HtmlImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));

        private SmtpClient ConfiguredSmtpClient() {
            var objsmtpClient = new SmtpClient();

            objsmtpClient.Host = MetadataProvider.GlobalProperty("email.smtp.host", true);

            var overriddenPort = MetadataProvider.GlobalProperty("email.smtp.port");
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }

            objsmtpClient.EnableSsl = "true".EqualsIc(MetadataProvider.GlobalProperty("email.smtp.enableSSL"));

            // Increase timeout value if needed - depended on site 
            var timeout = MetadataProvider.GlobalProperty("email.smtp.timeout");
            if (timeout != null) {
                objsmtpClient.Timeout = Int32.Parse(timeout);
            }

            var username = MetadataProvider.GlobalProperty("email.smtp.username");
            var password = MetadataProvider.GlobalProperty("email.smtp.password");
            if (username != null && password != null) {
                objsmtpClient.UseDefaultCredentials = false;
                objsmtpClient.Credentials = new NetworkCredential(username, password);
                objsmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            }

            return objsmtpClient;
        }

        public async void SendEmailAsync(EmailData emailData) {
            var smtpClient = ConfiguredSmtpClient();
            var email = BuildMailMessage(emailData);
            // Send the email message asynchronously
            await smtpClient.SendMailAsync(email)
                .ContinueWith(e => Log.Error(e), TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendEmail(EmailData emailData) {
            var smtpClient = ConfiguredSmtpClient();
            var email = BuildMailMessage(emailData);
            // Send the email message synchronously
            try {
                smtpClient.Send(email);
            } catch (Exception ex) {
                Log.Error(ex);
                throw;
            }
        }

        private MailMessage BuildMailMessage(EmailData emailData) {
            var email = new MailMessage() {
                From = new MailAddress(emailData.SendFrom ?? MetadataProvider.GlobalProperty("defaultEmail")),
                Subject = emailData.Subject,
                Body = emailData.Message,
                IsBodyHtml = true
            };

            var anyAddressSet = false;

            if (!string.IsNullOrEmpty((emailData.SendTo))) {
                foreach (
                    var emailaddress in emailData.SendTo.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.To.Add(emailaddress.Trim());
                        anyAddressSet = true;
                    }
                }
            }

            if (!anyAddressSet) {
                throw new InvalidOperationException(
                    "Email cannot be sent to {0}. Dev environments limit the domain which can be sent to avoid sending unadvertised test emails"
                        .Fmt(emailData.SendTo));
            }


            if (!string.IsNullOrEmpty(emailData.Cc)) {
                foreach (var emailaddress in emailData.Cc.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.CC.Add(emailaddress.Trim());
                    }
                }
            }

            if (!string.IsNullOrEmpty(emailData.BCc)) {
                foreach (var emailaddress in emailData.BCc.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (AllowedToAdd(emailaddress)) {
                        email.Bcc.Add(emailaddress.Trim());
                    }
                }
            }

            email.IsBodyHtml = true;
            if (emailData.Attachments != null) {
                HandleAttachments(emailData.Attachments, email);
            }
            return email;
        }

        private static Boolean AllowedToAdd(string emailaddress) {
            if (!ApplicationConfiguration.IsLocal()) {
                return true;
            }
            if (!new EmailAddressAttribute().IsValid(emailaddress)) {
                Log.WarnFormat("The email ( {0}) is not valid".Fmt(emailaddress));
                return false;
            }

            var domain = emailaddress.Split('@')[1];
            if (!domain.EqualsAny("controltechnologysolutions.com", "softwrenchsolutions.com", "amlabs.com.br")) {
                Log.WarnFormat("This email ( {0}) is not valid for this environment".Fmt(emailaddress));
                return false;
            }

            return true;
        }

        private void HandleAttachments(List<EmailAttachment> attachments, MailMessage email) {
            foreach (var attachment in attachments) {
                string encodedAttachment = attachment.AttachmentData.Substring(attachment.AttachmentData.IndexOf(",") + 1);
                byte[] bytes = Convert.FromBase64String(encodedAttachment);
                email.Attachments.Add(new Attachment(new MemoryStream(bytes), attachment.AttachmentName));
            }
        }

    }
}
