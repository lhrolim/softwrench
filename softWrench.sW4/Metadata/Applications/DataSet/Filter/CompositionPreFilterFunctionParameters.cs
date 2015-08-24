using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities.Sliced;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class CompositionPreFilterFunctionParameters : BasePreFilterParameters<ApplicationCompositionCollectionSchema> {
        private readonly SlicedEntityMetadata _slicedEntityMetadata;


        public CompositionPreFilterFunctionParameters(SlicedEntityMetadata slicedEntityMetadata, SearchRequestDto baseDto, AttributeHolder originalEntity, ApplicationCompositionCollectionSchema composition)
            : base(baseDto, composition, originalEntity) {
            _slicedEntityMetadata = slicedEntityMetadata;
        }

        public SlicedEntityMetadata Schema {
            get { return _slicedEntityMetadata; }
        }
    }
}
