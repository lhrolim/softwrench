using System;
using System.Text.RegularExpressions;
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
            try {
                objsmtpClient.Send(email);
            } catch (Exception ex) {
                Log.Error(ex);
                throw ex;
            }
        }

        public class EmailData {
            public EmailData(string sendFrom, string sendTo, string subject, string message) {
                Validate.NotNull(sendTo, "sentTo");
                Validate.NotNull(subject, "Subject");
                SendFrom = sendFrom;
                SendTo = sendTo;
                Subject = subject;
                Message = message;
            }


            public string SendFrom { get; set; }
            public string SendTo { get; set; }
            public string Cc { get; set; }
            public string Subject { get; set; }

            public string Message { get; set; }
        }


       
    }
}
