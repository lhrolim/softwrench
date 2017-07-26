using Newtonsoft.Json;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class CompositionSchemas {
        public CompositionSchemas() {
        }

        public CompositionSchemas(ApplicationSchemaDefinition detail, ApplicationSchemaDefinition list) {
            Detail = detail;
            List = list;
        }

        public ApplicationSchemaDefinition Detail { get; set; }

        public ApplicationSchemaDefinition DetailOutput { get; set; }

        public ApplicationSchemaDefinition List { get; set; }

        public ApplicationSchemaDefinition Print { get; set; }

        [JsonIgnore]
        public ApplicationSchemaDefinition Sync { get; set; }

    }
}
