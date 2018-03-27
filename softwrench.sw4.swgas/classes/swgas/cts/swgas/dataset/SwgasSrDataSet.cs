using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;

namespace softwrench.sw4.swgas.classes.swgas.cts.swgas.dataset {

    public class SwgasSrDataSet : BaseServiceRequestDataSet {

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id,
            string operation, bool isBatch, UserIdSiteOrg userIdSite,
            IDictionary<string, object> operationDataCustomParameters) {
            var schemaid = application.Schema.SchemaId;
            if (IsCreateOrUpdate(operation) && !isBatch)
            {
                var defaultOrg = MetadataProvider.GlobalProperty("defaultOrgId");
                var defaultSite = MetadataProvider.GlobalProperty("defaultSiteId");
                json.ReplaceValue("siteid", new JValue(defaultSite));
                json.ReplaceValue("orgid", new JValue(defaultOrg));
            }
            if (!IsCreateOrUpdate(operation) || !"nologinnewdetail".Equals(schemaid) || isBatch) {
                return await base.Execute(application, json, id, operation, false, userIdSite,
                    operationDataCustomParameters);
            }
            json.ReplaceValue("ld_.ldtext", new JValue(BuildDetails(json)));
            json.ReplaceValue("reportedemail", new JValue(json.StringValue("email")));
            json.ReplaceValue("description", new JValue(BuildSummary(json)));
            json.ReplaceValue("reportedby", new JValue(json.StringValue("name")));
            return await base.Execute(application, json, id, operation, false, userIdSite,
                operationDataCustomParameters);
        }

        private static string BuildDetails(JObject json) {
            var name = json.StringValue("name");
            var email = json.StringValue("email");
            var phoneext = json.StringValue("phoneext");
            var ldescription = json.StringValue("description");
            var urgency = json.StringValue("requrgency");
            var personnel = json.StringValue("affectpersonnel");
            var division = json.StringValue("division");
            var site = json.StringValue("city");
            var location = json.StringValue("locationb");
            var summary = json.StringValue("summary");
            return
                $"<b>Name:</b> {name}<br/><b>Email:</b> {email}<br/><b>Phone:</b> {phoneext}<br/><b>Urgency:</b>{urgency} <br/><b>Affected Personnel:</b>{personnel} <br/><b>Division:</b>{division} <br/><b>Site:</b>{site} <br/><b>Location:</b>{location} <br/><br/><b>Summary:</b>{summary} <br/><br/><b>Description:</b><br/>{ldescription}";
        }

        private static string BuildSummary(JObject json) {
            var name = json.StringValue("name");
            var phoneext = json.StringValue("phoneext");
            
            return $"[New Request from {name} : {phoneext}]";
        }

        private static bool IsCreateOrUpdate(string operation) {
            return string.Equals(operation, OperationConstants.CRUD_CREATE) ||
                   string.Equals(operation, OperationConstants.CRUD_UPDATE);
        }

        public override string ClientFilter() {
            return "swgas";
        }
    }


}

