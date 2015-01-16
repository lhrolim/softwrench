using softWrench.sW4.Data.Pagination;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API {

    public class CheckPointCrudContext {

        public ApplicationKey ApplicationKey { get; set; }

        public PaginatedSearchRequestDto ListContext { get; set; }


    }
}