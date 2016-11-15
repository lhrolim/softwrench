using softWrench.sW4.Data.Batches;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector {

    public class KeywordBatchConverter : ABatchSubmissionConverter {

        public override string BatchProperty {
            get { return "#keywordlist_"; }
        }

        public override string ApplicationName() {
            return "keyword";
        }

        public override string ClientFilter() {
            return null;
        }

        public override string SchemaId() {
            return null;
        }


    }
}
