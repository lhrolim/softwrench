using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.API.Association.Lookup {
    public class LookupOptionsFetchRequestDTO {

        public ApplicationMetadataSchemaKey ParentKey { get; set; }

        public ApplicationMetadataSchemaKey AssociationKey { get; set; }

        public string AssociationFieldName { get; set; }


        public PaginatedSearchRequestDto SearchDTO { get; set; }



    }


}
