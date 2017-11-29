using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchAcceptedEmailService : BaseDispatchStatusEmailService {

        public async Task SendEmail(DispatchTicket ticket) {
            var dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>();
            var site = await dao.FindSingleByQueryAsync<GfedSite>(GfedSite.FromGFedId, ticket.GfedId);
            var subject = "[#{0}] Ticket Accepted – {1}".Fmt(ticket.Id, site.FacilityName);
            if (!ApplicationConfiguration.IsProd()) {
                subject = "[{0}]".Fmt(ApplicationConfiguration.Profile) + subject;
            }
            SendEmail(ticket, site, subject);
        }

        protected override string GetTemplateFilePath() {
            return "//Content//Customers//firstsolardispatch//htmls//templates//dispatchaccepted.html";
        }
    }
}
