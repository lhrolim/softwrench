using System.Threading.Tasks;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagProblemDataSet : MaximoApplicationDataSet {
        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dbDetail = await base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (application.Schema.Mode == SchemaMode.output) {
                HandleClosedDate(resultObject, application);
            }
            return dbDetail;
        }

        private void HandleClosedDate(DataMap resultObject, ApplicationMetadata application) {
            var status = resultObject.GetAttribute("status");
            var statusDate = resultObject.GetAttribute("statusdate");
            if ("CLOSED".Equals(status)) {
                resultObject["#closeddate"] = statusDate;
            } else {
                resultObject["#closeddate"] = "";
            }
        }

        public override string ApplicationName() {
            return "problem";
        }

        public override string ClientFilter() {
            return "hapag";
        }
    }
}
