using softWrench.sW4.Data.Batches;

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
