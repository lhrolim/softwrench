using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sw4.api.classes.fwk.dataset;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.dataset {
    public class PescoServiceRequestDataSet : BaseServiceRequestDataSet {

        public PescoServiceRequestDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        public override string ClientFilter() {
            return "pesco";
        }

        [PreFilter("worklog")]
        public virtual SearchRequestDto WorkLogPreFilter(CompositionPreFilterFunctionParameters param) {
            var originalDTO = param.BASEDto;
            var user = SecurityFacade.CurrentUser();
            if (!user.IsInRole("worklogclientviewable")) {
                originalDTO.AppendWhereClause("clientviewable = 1");
            }
            return originalDTO;
        }

        public override TargetResult DoExecute(OperationWrapper operationWrapper)
        {
            var data = operationWrapper.GetOperationData;
            if (!data.ApplicationMetadata.Name.Equals("quickservicerequest") && !true.Equals(data.Holder.GetAttribute("lostenergy3"))) {
                data.Holder.SetAttribute("lostenergyamount","");
            }
            return base.DoExecute(operationWrapper);
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
