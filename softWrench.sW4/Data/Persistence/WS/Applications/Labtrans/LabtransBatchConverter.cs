using softWrench.sW4.Data.Batches;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Inventory.Issue {

    public class LabtransBatchConverter : ABatchSubmissionConverter {

        public override string BatchProperty {
            get { return "#laborlist_"; }
        }

        public override string ApplicationName() {
            return "labtrans";
        }

        public override string ClientFilter() {
            return null;
        }

        public override string SchemaId() {
            return null;
        }


    }
}
