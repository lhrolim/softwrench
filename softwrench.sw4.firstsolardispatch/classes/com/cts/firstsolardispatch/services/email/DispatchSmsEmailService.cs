using System.Collections.Generic;
using System.Threading.Tasks;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchSmsEmailService : BaseDispatchEmailService {


        protected override string GetTemplateFilePath() {
            return "//Content//Customers//firstsolardispatch//htmls//templates//dispatchsms.html";
        }

        public override string BuildTo(GfedSite site, int hour) {
            var toList = new List<string>();

            if (!string.IsNullOrEmpty(site.PrimaryContactEmail)) {
                toList.Add(site.PrimaryContactEmail);
            }
            if (!string.IsNullOrEmpty(site.EscalationContactEmail) && hour > 0) {
                toList.Add(site.EscalationContactEmail);
            }
            if (hour > 1 && ApplicationConfiguration.IsProd()) {
                toList.Add("frank.kelly@firstsolar.com");
            }

            toList.Add(GetSmsEmails());

            return string.Join("; ", toList);
        }
    }
}
