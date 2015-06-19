using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.InvIssue {

    public class InvIssueBatchConverter : IBatchSubmissionConverter<ApplicationMetadata,OperationWrapper> {

        private readonly EntityMetadata _entityMetadata;

        public string ApplicationName() {
            return "invissue";
        }

        public string ClientFilter() {
            return null;
        }

        public string SchemaId() {
            return null;
        }

        public InvIssueBatchConverter() {
            _entityMetadata = MetadataProvider.Entity("invissue");
        }

        public JArray BreakIntoRows(JObject mainDatamap) {
            dynamic obj = mainDatamap;
            var rows = obj["#batchitem_"];
            mainDatamap.Remove("#batchitem_");

            var result = new JArray();

            foreach (dynamic row in rows) {
                var batchItem = new JObject(mainDatamap);
                JSonUtil.ReplaceValue(batchItem, "storeloc", row["storeloc"]);
                JSonUtil.ReplaceValue(batchItem, "binnum", row["binnum"]);
                JSonUtil.ReplaceValue(batchItem, "quantity", row["quantity"]);
                result.Add(batchItem);
            }
            return result;
        }

        public bool ShouldSubmit(JObject row) {
            return true;
        }

        public OperationWrapper Convert(JObject row, ApplicationMetadata applicationMetadata)
        {
            var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, applicationMetadata, row, null);
            return new OperationWrapper(crudOperationData,OperationConstants.CRUD_CREATE);
        }
    }
}
