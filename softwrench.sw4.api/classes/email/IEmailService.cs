using cts.commons.simpleinjector;

namespace softwrench.sw4.api.classes.email {
    public interface IEmailService : ISingletonComponent {

        void SendEmail(EmailData emailData);
    }
}