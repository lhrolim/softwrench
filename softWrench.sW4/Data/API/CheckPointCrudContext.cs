using softWrench.sW4.Data.Pagination;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Util;

namespace softWrench.sW4.Data.API {

    public class CheckPointCrudContext {

        public string ApplicationKey { get; set; }

        public PaginatedSearchRequestDto ListContext { get; set; }

        public ApplicationKey GetApplicationKey() {
            return new ApplicationKey(SchemaUtil.ParseApplicationAndKey(ApplicationKey));
        }


    }
}