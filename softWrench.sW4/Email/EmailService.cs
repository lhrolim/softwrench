using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Math;
using softWrench.sW4.SimpleInjector;
using System.Net.Mail;
using softWrench.sW4.Metadata;
using Common.Logging;
using softWrench.sW4.Util;


namespace softWrench.sW4.Email {
    public class EmailService : ISingletonComponent {

        private static readonly Regex HtmlImgRegex = new Regex("<img[^>]+src\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");

        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailService));
        public void SendEmail(EmailData emailData) {
            var objsmtpClient = new SmtpClient();
            objsmtpClient.Host = MetadataProvider.GlobalProperty("email.smtp.host", true);
            var overriddenPort = MetadataProvider.GlobalProperty("email.smtp.port");
            if (overriddenPort != null) {
                objsmtpClient.Port = Int32.Parse(overriddenPort);
            }
            objsmtpClient.EnableSsl = "true".EqualsIc(MetadataProvider.GlobalProperty("email.stmp.enableSSL", true));
            // Send the email message
            var email = new MailMessage(emailData.SendFrom, emailData.SendTo) {
                Subject = emailData.Subject,
                Body = emailData.Message,
                IsBodyHtml = true
            };
            if (emailData.Cc != null) {
                foreach (var ccemail in emailData.Cc.Split(' ', ',', ';')) {
                    email.CC.Add(ccemail);
                }
            }
            email.IsBodyHtml = true;
            if (emailData.AttachmentData != null){
                string encodedAttachment = emailData.AttachmentData.Substring(emailData.AttachmentData.IndexOf(",") + 1);
                byte[] bytes = Convert.FromBase64String(encodedAttachment);
                email.Attachments.Add(new Attachment(new MemoryStream(bytes), emailData.AttachmentName));
            }

            try {
                objsmtpClient.Send(email);
            } catch (Exception ex) {
                Log.Error(ex);
                throw ex;
            }
        }

        public class EmailData {
            public EmailData(string sendFrom, string sendTo, string subject, string attachmentData, string attachmentName, string message) {
                Validate.NotNull(sendTo, "sentTo");
                Validate.NotNull(subject, "Subject");
                SendFrom = sendFrom;
                SendTo = sendTo;
                Subject = subject;
                Message = message;
                AttachmentData = attachmentData;
                AttachmentName = attachmentName;
            }


            public string SendFrom { get; set; }
            public string SendTo { get; set; }
            public string Cc { get; set; }
            public string Subject { get; set; }
            public string AttachmentData { get; set; }
            public string AttachmentName { get; set; }
            public string Message { get; set; }
        }


       
    }
}
