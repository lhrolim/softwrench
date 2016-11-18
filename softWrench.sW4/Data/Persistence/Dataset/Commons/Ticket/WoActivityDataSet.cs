using System;
using System.Threading.Tasks;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    class WorActivityDataSet : BaseTicketDataSet {


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            //default=@now is not working for edition, and we cannot pick it from maximo side
            result.ResultObject.SetAttribute("#newstatusdate", DateTime.Now);
            return result;
        }

        public override string ApplicationName() {
            return "woactivity";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
