namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class UsefulFaqLinksUtils {
        private readonly string _id;
        private readonly string _language;
        private readonly string _schemaId;

        public UsefulFaqLinksUtils(string id, string language, string schemaId) {
            _id = id;
            _language = language;
            _schemaId = schemaId;
        }

        public string Id {
            get { return _id; }
        }

        public string Language {
            get { return _language; }
        }

        public string SchemaId {
            get { return _schemaId; }
        }
    }
}
