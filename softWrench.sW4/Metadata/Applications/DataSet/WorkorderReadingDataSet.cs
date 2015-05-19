using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    class WorkorderReadingDataSet : MaximoApplicationDataSet {
        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = base.GetApplicationDetail(application, user, request);
            var locationMeter = (List<Dictionary<string, object>>)detail.ResultObject.GetAttribute("locationmeter_");
            var assetMeter = (List<Dictionary<string, object>>)detail.ResultObject.GetAttribute("assetmeter_");
            if (!locationMeter.Any() && !assetMeter.Any()) {
                detail.ExtraParameters.Add("exception", "The asset and location do not have any active meters associated with them.");
            }
            return detail;
        }


        public override string ApplicationName() {
            return "workorder";
        }

        public override string SchemaId() {
            return "readings";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
