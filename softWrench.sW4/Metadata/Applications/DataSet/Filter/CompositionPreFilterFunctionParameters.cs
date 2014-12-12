using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class CompositionPreFilterFunctionParameters : BasePreFilterParameters<ApplicationCompositionCollectionSchema> {
        private readonly ApplicationSchemaDefinition _appSchema;


        public CompositionPreFilterFunctionParameters(ApplicationSchemaDefinition appSchema, SearchRequestDto baseDto, AttributeHolder originalEntity, ApplicationCompositionCollectionSchema composition)
            : base(baseDto, composition, originalEntity) {
            _appSchema = appSchema;
        }

        public ApplicationSchemaDefinition AppSchema {
            get { return _appSchema; }
        }
    }
}
