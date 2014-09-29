using softwrench.sW4.Shared.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions {
    public class CompositionSchemas {
        public CompositionSchemas() {

        }

        public CompositionSchemas(ApplicationSchemaDefinition detail, ApplicationSchemaDefinition list) {
            Detail = detail;
            List = list;
        }

        public ApplicationSchemaDefinition Detail { get; set; }

        public ApplicationSchemaDefinition List { get; set; }
    }
}
