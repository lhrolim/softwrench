using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class BasePreFilterParameters<TRelationhipmetadata> {

        
        private SearchRequestDto _baseDto;
        private readonly AttributeHolder _originalEntity;
        private readonly TRelationhipmetadata _relationship;


        public BasePreFilterParameters(SearchRequestDto baseDto,
            TRelationhipmetadata relationship, AttributeHolder originalEntity) {
            _baseDto = baseDto;
            _originalEntity = originalEntity;
            _relationship = relationship;
        }

     

        public SearchRequestDto BASEDto
        {
            get { return _baseDto; }
            set { _baseDto = value; }
        }


        public AttributeHolder OriginalEntity {
            get { return _originalEntity; }
        }


        public TRelationhipmetadata Relationship {
            get { return _relationship; }
        }
    }
}
