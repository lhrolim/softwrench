
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchRequest {

        public string Id { get; set; }

        public ApplicationMetadataSchemaKey Key { get; set; }
    }
}
