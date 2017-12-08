using System.Collections.Generic;
using System.Threading.Tasks;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchEmailService : BaseDispatchEmailService {

        protected override string GetTemplateFilePath() {
            return "//Content//Customers//firstsolardispatch//htmls//templates//dispatch.html";
        }

        public override string BuildTo(GfedSite site, int hour) {
            var toList = new List<string>();

            if (!string.IsNullOrEmpty(site.PrimaryContactSmsEmail)) {
                toList.Add(site.PrimaryContactEmail);
            }
            if (!string.IsNullOrEmpty(site.EscalationContactSmsEmail) && hour > 0) {
                toList.Add(site.EscalationContactEmail);
            }

            return string.Join("; ", toList);
        }
    }
}
