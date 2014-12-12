using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class CompositionPreFilterFunctionParameters : BasePreFilterParameters<ApplicationCompositionCollectionSchema> {


        public CompositionPreFilterFunctionParameters(SearchRequestDto baseDto,
            AttributeHolder originalEntity, ApplicationCompositionCollectionSchema composition)
            : base(baseDto, composition, originalEntity) {
        }

    }
}
