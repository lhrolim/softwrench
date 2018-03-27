using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;

namespace softWrench.sW4.umc.classes.com.cts.umc.dataset {
    public class UmcSRDataSet : BaseServiceRequestDataSet {

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, UserIdSiteOrg userIdSite, IDictionary<string, object> operationDataCustomParameters) {
            var schemaid = application.Schema.SchemaId;
            if (IsCreateOrUpdate(operation) && !isBatch) {
                json.ReplaceValue("siteid", new JValue("UMCSITE"));
                json.ReplaceValue("orgid", new JValue("UMCORG"));
            }
            if (!IsCreateOrUpdate(operation) || !"nologinnewdetail".Equals(schemaid) || isBatch){
                return await base.Execute(application, json, id, operation, false, userIdSite, operationDataCustomParameters);
            }
            json.ReplaceValue("ld_.ldtext", new JValue(BuildDetails(json)));
            json.ReplaceValue("reportedemail", new JValue(json.StringValue("email")));
            json.ReplaceValue("description", new JValue(BuildSummary(json)));
            return await base.Execute(application, json, id, operation, false, userIdSite, operationDataCustomParameters);
        }

        private static string BuildDetails(JObject json) {
            var name = json.StringValue("name");
            var email = json.StringValue("email");
            var phoneext = json.StringValue("phoneext");
            var reqsummary = json.StringValue("description");
            var location = json.StringValue("customlocation");
            return $"<b>Name:</b> {name}<br/><b>Email:</b> {email}<br/><b>Phone Extension:</b> {phoneext}<br/><b>Location:</b>{location}<br/><br/><b>Request Summary:</b><br/>{reqsummary}";
        }

        private static string BuildSummary(JObject json) {
            var name = json.StringValue("name");
            var phoneext = json.StringValue("phoneext");
            return $"[New Request from {name} x{phoneext}]";
        }

        private static bool IsCreateOrUpdate(string operation) {
            return string.Equals(operation, OperationConstants.CRUD_CREATE) || string.Equals(operation, OperationConstants.CRUD_UPDATE);
        }

        public override string ClientFilter() {
            return "umc";
        }
    }
}
