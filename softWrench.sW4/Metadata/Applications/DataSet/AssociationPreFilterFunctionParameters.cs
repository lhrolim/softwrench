using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class AssociationPreFilterFunctionParameters {
        private readonly ApplicationMetadata _metadata;
        private readonly SearchRequestDto _baseDto;
        private readonly ApplicationAssociationDefinition _association;
        private readonly AttributeHolder _originalEntity;

        public AssociationPreFilterFunctionParameters(ApplicationMetadata metadata, SearchRequestDto baseDto,
            ApplicationAssociationDefinition association, AttributeHolder originalEntity) {
            _metadata = metadata;
            _baseDto = baseDto;
            _association = association;
            _originalEntity = originalEntity;
        }

        public ApplicationMetadata Metadata {
            get { return _metadata; }
        }

        public SearchRequestDto BASEDto {
            get { return _baseDto; }
        }

        public ApplicationAssociationDefinition ASSOCIATION {
            get { return _association; }
        }

        public AttributeHolder OriginalEntity {
            get { return _originalEntity; }
        }
    }
}
