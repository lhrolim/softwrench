using Newtonsoft.Json.Linq;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.services;
using softWrench.sW4.Data.Batches;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Inventory.Issue {

    public class InvIssueBatchConverter : ABatchSubmissionConverter {

        public override string BatchProperty {
            get { return "#batchitem_"; }
        }

        public override string ApplicationName() {
            return "invissue";
        }

        public override string ClientFilter() {
            return null;
        }

        public override string SchemaId() {
            return null;
        }


    }
}
