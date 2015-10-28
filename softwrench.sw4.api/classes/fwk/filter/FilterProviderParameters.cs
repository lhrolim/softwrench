namespace softwrench.sw4.api.classes.fwk.filter {
    public class FilterProviderParameters {

        public FilterProviderParameters(string inputSearch, string filterAttribute, string schemaId) {
            InputSearch = inputSearch;
            FilterAttribute = filterAttribute;
            SchemaId = schemaId;
        }

        public string InputSearch {
            get; set;
        }

        public string FilterAttribute {
            get; set;
        }

        public string SchemaId {
            get; set;
        }

    }
}
