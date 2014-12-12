using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class CompositionPreFilterFunctionParameters : BasePreFilterParameters<ApplicationCompositionCollectionSchema> {
        private readonly EntitySchema _schema;


        public CompositionPreFilterFunctionParameters(EntitySchema schema, SearchRequestDto baseDto, AttributeHolder originalEntity, ApplicationCompositionCollectionSchema composition)
            : base(baseDto, composition, originalEntity) {
            _schema = schema;
        }

        public EntitySchema Schema {
            get { return _schema; }
        }
    }
}
