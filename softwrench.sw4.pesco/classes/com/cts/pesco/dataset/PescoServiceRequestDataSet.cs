using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.dataset {
    public class PescoServiceRequestDataSet : BaseServiceRequestDataSet {

        public PescoServiceRequestDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        public override string ClientFilter() {
            return "pesco";
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            result.ResultObject.ToList().ForEach(CopyStatus);
            return result;
        }

        private static void CopyStatus(AttributeHolder datamap) {
            var status = datamap.GetAttribute("status");
            datamap.SetAttribute("pending", status);
        }
    }
}
