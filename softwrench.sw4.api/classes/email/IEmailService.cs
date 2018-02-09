using System;
using System.Net.Mail;
using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.email {
    public interface IEmailService : ISingletonComponent {

        void SendEmail(EmailData emailData, SmtpClient smtpClient = null);

        /// <summary>
        /// Sends email in a fire-and-forget way.
        /// </summary>
        /// <param name="emailData"></param>
        /// <param name="callback"></param>
        void SendEmailAsync(EmailData emailData, Action<bool> callback= null);

        EmailAttachment CreateAttachment(string fileContent, string attachmentName);

        EmailAttachment CreateAttachment(byte[] fileContent, string attachmentName);
    }
}