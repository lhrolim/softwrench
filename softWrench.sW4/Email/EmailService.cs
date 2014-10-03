using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Configuration.Services.Api;
using System.Net.Mail;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Metadata;
using Common.Logging;
using softWrench.sW4.Util;

namespace softWrench.sW4.Email {
    public class EmailService : ISingletonComponent {

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
            };
            if (emailData.Cc != null) {
                foreach (var ccemail in emailData.Cc.Split(' ',',',';')){
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
