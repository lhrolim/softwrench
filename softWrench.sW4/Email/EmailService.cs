using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using cts.commons.portable.Util;
using cts.commons.Util;
using DocumentFormat.OpenXml.Math;
using cts.commons.simpleinjector;
using System.Net.Mail;
using softWrench.sW4.Metadata;
using Common.Logging;
using softWrench.sW4.Util;


namespace softWrench.sW4.Email {
    public class EmailService : ISingletonComponent {

        private static readonly Regex HtmlImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));

        private SmtpClient ConfiguredSmtpClient() {
            var objsmtpClient = new SmtpClient();

            objsmtpClient.Host = MetadataProvider.GlobalProperty("email.smtp.host", true);

            var overriddenPort = MetadataProvider.GlobalProperty("email.smtp.port");
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }

            objsmtpClient.EnableSsl = "true".EqualsIc(MetadataProvider.GlobalProperty("email.stmp.enableSSL", true));

            // Increase timeout value if needed - depended on site 
            var timeout = MetadataProvider.GlobalProperty("email.smtp.timeout");
            if (timeout != null) {
                objsmtpClient.Timeout = Int32.Parse(timeout);
            }

            return objsmtpClient;
        }

        public void SendEmail(EmailData emailData) {
            var objsmtpClient = ConfiguredSmtpClient();
           
            // Send the email message
            var email = new MailMessage() {
                From = new MailAddress(emailData.SendFrom ?? MetadataProvider.GlobalProperty("defaultEmail")),
                Subject = emailData.Subject,
                Body = emailData.Message,
                IsBodyHtml = true
            };

            if (!string.IsNullOrEmpty((emailData.SendTo))) {
                foreach (var emailaddress in emailData.SendTo.Split(new char[]{' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries)) {
                    email.To.Add(emailaddress.Trim());
                }
            }

            if (!string.IsNullOrEmpty(emailData.Cc)) {
                foreach (var emailaddress in emailData.Cc.Split(new char[]{' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries)) {
                    email.CC.Add(emailaddress.Trim());
                }
            }
            email.IsBodyHtml = true;
            if (emailData.Attachments != null){
                HandleAttachments(emailData.Attachments, email);
            }

            try
            {
                objsmtpClient.Send(email);
            } catch (Exception ex) {
                Log.Error(ex);
                throw ex;
            }
        }

        private void HandleAttachments(List<EmailAttachment> attachments, MailMessage email) {
            foreach (var attachment in attachments){
                string encodedAttachment = attachment.AttachmentData.Substring(attachment.AttachmentData.IndexOf(",") + 1);
                byte[] bytes = Convert.FromBase64String(encodedAttachment);
                email.Attachments.Add(new Attachment(new MemoryStream(bytes), attachment.AttachmentName));
            }
            
        }

        public class EmailData {
            public EmailData(string sendFrom, string sendTo, string subject, string message, List<EmailAttachment> attachments = null) {
                Validate.NotNull(sendTo, "sentTo");
                Validate.NotNull(subject, "Subject");
                SendFrom = sendFrom;
                SendTo = sendTo;
                Subject = subject;
                Message = message;
                Attachments = attachments;
            }

            public string SendFrom { get; set; }
            public string SendTo { get; set; }
            public string Cc { get; set; }
            public string Subject { get; set; }
            public List<EmailAttachment> Attachments { get; set; }
            public string Message { get; set; }
        }

        public class EmailAttachment{
            public EmailAttachment(string attachmentData, string attachmentName){
                AttachmentData = attachmentData;
                AttachmentName = attachmentName;
            }

            public string AttachmentData { get; set; }
            public string AttachmentName { get; set; }
        }


       
    }
}
