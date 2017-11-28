using System.Threading.Tasks;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services.email {
    public class DispatchSmsEmailService : BaseDispatchEmailService {


        protected override string GetTemplateFilePath() {
            return "//Content//Customers//firstsolardispatch//htmls//templates//dispatchsms.html";
        }
    }
}
