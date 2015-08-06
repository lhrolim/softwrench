using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {

    public class AssociationPreFilterFunctionParameters : BasePreFilterParameters<ApplicationAssociationDefinition> {

        private readonly ApplicationMetadata _metadata;

        public AssociationPreFilterFunctionParameters(ApplicationMetadata metadata, SearchRequestDto baseDto,
            ApplicationAssociationDefinition association, AttributeHolder originalEntity)
            : base(baseDto, association, originalEntity) {
            _metadata = metadata;
        }

        public ApplicationMetadata Metadata {
            get { return _metadata; }
        }
    }
}
