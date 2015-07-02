using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Metadata.Applications.DataSet {

    class WorkorderReadingDataSet : MaximoApplicationDataSet {
        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var currentData = new JObject
            {
                {"assetnum", request.InitialValues.GetAttribute("assetnum") as string},
                {"location", request.InitialValues.GetAttribute("location") as string}
            };
            var compositionData = base.GetCompositionData(application, new CompositionFetchRequest {
                Id = request.Id,
                CompositionList = new List<string> { "locationmeter_", "assetmeter_" }
            }, currentData);
            var locationMeter = (List<Dictionary<string, object>>)compositionData.ResultObject["locationmeter_"].ResultList;
            var assetMeter = (List<Dictionary<string, object>>)compositionData.ResultObject["assetmeter_"].ResultList;
            var datamap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, null);
            datamap.SetAttribute("locationmeter_", locationMeter);
            datamap.SetAttribute("assetmeter_", assetMeter);
            var compositions = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var detail = new ApplicationDetailResult(datamap, null, application.Schema, compositions, request.Id);
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
