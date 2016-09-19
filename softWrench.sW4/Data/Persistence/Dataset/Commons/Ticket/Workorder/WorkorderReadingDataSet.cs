using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder {

    class WorkorderReadingDataSet : MaximoApplicationDataSet {
        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var datamapToUse = LocateDataMap(request.InitialValues);


            var currentData = new JObject
            {
                {"assetnum", datamapToUse.GetAttribute("assetnum") as string},
                {"location", datamapToUse.GetAttribute("location") as string},
                {"siteid", datamapToUse.GetAttribute("siteid") as string}
            };
            var compositionData = await base.GetCompositionData(application, new CompositionFetchRequest {
                Id = request.Id,
                CompositionList = new List<string> { "locationmeter_", "assetmeter_" }
            }, currentData);
            var datamap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, null);

            var locationMeter = new List<Dictionary<string, object>>();

            if (compositionData.ResultObject.ContainsKey("locationmeter_")) {
                //might be turned off
                locationMeter = (List<Dictionary<string, object>>)compositionData.ResultObject["locationmeter_"].ResultList;
                datamap.SetAttribute("locationmeter_", locationMeter);
            }

            var assetMeter = (List<Dictionary<string, object>>)compositionData.ResultObject["assetmeter_"].ResultList;


            datamap.SetAttribute("assetmeter_", assetMeter);
            var compositions = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            var detail = new ApplicationDetailResult(datamap, null, application.Schema, compositions, request.Id);
            if (!locationMeter.Any() && !assetMeter.Any()) {
                detail.ExtraParameters.Add("exception", "The asset and location do not have any active meters associated with them.");
            }
            return detail;
        }

        /// <summary>
        /// We´ll either use the workorder itself, or a multiassetlocci if any selected
        /// </summary>
        /// <param name="initialValues"></param>
        /// <returns></returns>
        private Entity LocateDataMap(Entity initialValues) {

            var relationship = (IEnumerable<Entity>)initialValues.GetRelationship("multiassetlocci");
            if (relationship == null) {
                return initialValues;
            }
            foreach (var multiAssetLocci in relationship) {
                if ("true".Equals(multiAssetLocci.GetUnMappedAttribute("#selected"))) {
                    return multiAssetLocci;
                }
            }

            return initialValues;
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
