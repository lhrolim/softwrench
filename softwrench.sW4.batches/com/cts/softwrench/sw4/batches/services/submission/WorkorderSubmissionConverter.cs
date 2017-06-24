using System.Collections.Generic;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission {

    //TODO: make this specific to TVA, and modify SimpleInjector
    public class WorkorderBatchSubmissionConverter : IBatchSubmissionConverter<ApplicationMetadata, OperationWrapper> {
        public JArray BreakIntoRows(JObject mainDatamap) {
            var dataMapJsonAsString = mainDatamap["datamap"].ToString();
            return JArray.Parse(dataMapJsonAsString);
        }

        public bool ShouldSubmit(JObject row) {
            return "true".EqualsIc(row.StringValue("#closed"));
        }

        public OperationWrapper Convert(JObject row, ApplicationMetadata metadata) {
            var completeApp = MetadataProvider.Application("workorder");
            var app = completeApp.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var entityMetadata = MetadataProvider.Entity("workorder");
            var id = row.StringValue("wonum");
            var crudOperationData = new CrudOperationData(id, BuildAttributes(row), BuildRelationships(row), entityMetadata, app);
            return new OperationWrapper(crudOperationData, OperationConstants.CRUD_UPDATE);
        }

        private static Dictionary<string, object> BuildRelationships(JObject row) {
            var relationships = new Dictionary<string, object>();
            if ("Y".EqualsIc(row.StringValue("#lognote"))) {
                //TODO: try to sue EntityBuilder
                var jprop = row.Property("worklog_").Value as JObject;
                var worklogs = new List<CrudOperationData>();
                var entityMetadata = MetadataProvider.Entity("worklog");
                var completeApp = MetadataProvider.Application("worklog");
                var app = completeApp.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
                var attributes = new Dictionary<string, object>();
                attributes["longdescription_.ldtext"] = jprop.StringValue("longdescription_.ldtext");
                attributes["description"] = jprop.StringValue("description");
                worklogs.Add(new CrudOperationData(null, attributes, new Dictionary<string, object>(), entityMetadata, app));
                relationships.Add("worklog_", worklogs);
            }

            return relationships;
        }

        private static Dictionary<string, object> BuildAttributes(JObject row) {
            var dict = new Dictionary<string, object>();
            dict["ACTFINISH"] = ConversionUtil.HandleDateConversion(row.StringValue("actfinish"),false);
            //now we can finish the mapping using target file
            dict["#ReconCd"] = row.StringValue("#ReconCd");
            dict["#pmchange"] = "y".EqualsIc(row.StringValue("#pmchange"));
            dict["#fdbckcomment"] = row.StringValue("#fdbckcomment");
            return dict;
        }

        public string ApplicationName() {
            return "workorder";
        }

        public string SchemaId() {
            return null;
        }

        public string ClientFilter() {
            return "tva";
        }

        private void HandleLogNote(AttributeHolder item, JObject ob) {
            var jprop = ob.Property("worklog_");
            if (jprop != null) {
                item.SetAttribute("#lognote", "Y");
                item.SetAttribute("worklog_", jprop.Value);
            } else {
                item.SetAttribute("#lognote", "N");
            }

        }
    }
}
