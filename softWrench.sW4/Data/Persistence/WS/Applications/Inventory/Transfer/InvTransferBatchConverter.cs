using softWrench.sW4.Data.Batches;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Inventory.Transfer {

    public class InvTrasnferBatchConverter : ABatchSubmissionConverter {

        public override string BatchProperty {
            get { return "#batchitem_"; }
        }

        public override string ApplicationName() {
            return "invuse";
        }

        public override string ClientFilter() {
            return null;
        }

        public override string SchemaId() {
            return null;
        }

      

       

    }
}
