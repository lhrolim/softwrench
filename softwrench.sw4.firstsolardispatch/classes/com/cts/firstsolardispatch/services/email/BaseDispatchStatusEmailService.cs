using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.email;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Email;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public abstract class BaseDispatchStatusEmailService : BaseDispatchGenericEmailService {
        protected void SendEmail(DispatchTicket ticket, GfedSite site, string subject) {
            if (string.IsNullOrEmpty(ticket.ReportedBy?.Email) && string.IsNullOrEmpty(ticket.ReportedBy?.Person?.Email)) {
                return;
            }

            var to = ticket.ReportedBy?.Email ?? ticket.ReportedBy?.Person?.Email;
            var from = GetFrom();
            var msg = BuildMessage(ticket, site);
            var emailData = new EmailData(from, to, subject, msg);
            emailData.BCc += SwConstants.DevTeamEmail;

            var emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>();
            emailService.SendEmailAsync(emailData);
        }

        public string BuildMessage(DispatchTicket ticket, GfedSite site) {
            var redirectService = SimpleInjectorGenericFactory.Instance.GetObject<RedirectService>();
            var data = new {
                id = ticket.Id,
                sitename = site.FacilityName,
                ticketurl = redirectService.GetApplicationUrl("_DispatchTicket", "editdetail", "input", ticket.Id.ToString())
            };
            return BuildMessage(data);
        }
    }
}
