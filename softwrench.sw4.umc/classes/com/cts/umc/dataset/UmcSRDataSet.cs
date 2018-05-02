using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
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
                var r = await base.Execute(application, json, id, operation, false, userIdSite, operationDataCustomParameters);
                if (r != null && "DispatchWO".Equals(operation))
                {
                    dynamic srId = json["crud"][application.IdFieldName];
                    var ow = await BuildSetSRStatusWrapper(srId.ToString(), r.UserId, r.SiteId);
                    Engine().Execute(ow);
                }

                return r;
            }
            json.ReplaceValue("ld_.ldtext", new JValue(BuildDetails(json)));
            json.ReplaceValue("reportedemail", new JValue(json.StringValue("email")));
            json.ReplaceValue("description", new JValue(BuildSummary(json)));
            return await base.Execute(application, json, id, operation, false, userIdSite, operationDataCustomParameters);
        }

        private async Task<OperationWrapper> BuildSetSRStatusWrapper(string id, string userId, string siteId) {
            var schema = MetadataProvider.Application("servicerequest").Schema(new ApplicationMetadataSchemaKey("editdetail"));
            var sem = MetadataProvider.SlicedEntityMetadata(schema);

            DataMap dm = (DataMap)await Engine().FindById(sem, id, new Tuple<string, string>(userId, siteId));
            dynamic fields = new JObject();
            fields = JObject.FromObject(dm.Fields);
            var json = ((JObject) fields);
            json.ReplaceValue("status", new JValue("INPROG"));
            json.ReplaceValue("#hasstatuschange", new JValue(true));

            //dynamic jo = new JObject();
            //jo.crud = fields;

            return new OperationWrapper(ApplicationMetadata.FromSchema(schema), sem, "crud_update", json, null);
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
