using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class BasePreFilterParameters<TRelationhipmetadata> {

        
        private readonly SearchRequestDto _baseDto;
        private readonly AttributeHolder _originalEntity;
        private readonly TRelationhipmetadata _relationship;


        public BasePreFilterParameters(SearchRequestDto baseDto,
            TRelationhipmetadata relationship, AttributeHolder originalEntity) {
            _baseDto = baseDto;
            _originalEntity = originalEntity;
            _relationship = relationship;
        }

     

        public SearchRequestDto BASEDto {
            get { return _baseDto; }
        }


        public AttributeHolder OriginalEntity {
            get { return _originalEntity; }
        }


        public TRelationhipmetadata Relationship {
            get { return _relationship; }
        }
    }
}
