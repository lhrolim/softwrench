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
    public class PescoWorkorderderDataSet : BaseWorkorderDataSet {

        public PescoWorkorderderDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {
        }

        public override string ClientFilter() {
            return "pesco";
        }

        public override Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var data = operationWrapper.GetOperationData;
            if (!true.Equals(data.Holder.GetAttribute("lostenergy"))) {
                data.Holder.SetAttribute("lostenergyamount", "");
            }
            return base.DoExecute(operationWrapper);
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
    
    }
}
