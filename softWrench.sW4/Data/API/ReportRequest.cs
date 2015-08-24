using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using System;

namespace softWrench.sW4.Data.API {
    public class ReportRequest : DataRequestAdapter {

        public String ReportName { get; set; }        
        public String Application { get; set; }        

        public ReportRequest() : base() {
        }

        public ReportRequest(PaginatedSearchRequestDto searchDTO)
            : base(searchDTO) {
        }

        public ReportRequest(PaginatedSearchRequestDto searchDTO, ApplicationMetadataSchemaKey key)
            : base(searchDTO, key) {
        }
    }
}
