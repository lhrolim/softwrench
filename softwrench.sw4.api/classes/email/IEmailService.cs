using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.email {
    public interface IEmailService : ISingletonComponent {

        void SendEmail(EmailData emailData);
        
        /// <summary>
        /// Sends email in a fire-and-forget way.
        /// </summary>
        /// <param name="emailData"></param>
        void SendEmailAsync(EmailData emailData);
    }
}