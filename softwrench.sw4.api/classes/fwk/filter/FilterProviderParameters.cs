using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.api.classes.fwk.filter {
    public class FilterProviderParameters {

        public FilterProviderParameters(string inputSearch, string filterAttribute, ApplicationSchemaDefinition schema) {
            InputSearch = inputSearch;
            FilterAttribute = filterAttribute;
            ParentSchema = schema;
        }

        public string InputSearch {
            get; set;
        }

        public string FilterAttribute {
            get; set;
        }

        public ApplicationSchemaDefinition ParentSchema {
            get; set;
        }

    }
}
