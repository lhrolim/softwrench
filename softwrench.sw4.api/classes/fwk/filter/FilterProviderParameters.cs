using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.api.classes.fwk.filter {
    public class FilterProviderParameters {

        public FilterProviderParameters(string inputSearch, string filterAttribute, ApplicationMetadataSchemaKey schemaKey) {
            InputSearch = inputSearch;
            FilterAttribute = filterAttribute;
            SchemaKey = schemaKey;
        }

        public string InputSearch {
            get; set;
        }

        public string FilterAttribute {
            get; set;
        }

        public ApplicationMetadataSchemaKey SchemaKey { get; set; }

    }
}
