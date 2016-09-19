using System.Linq;
using System.Threading.Tasks;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {
    class BaseInventoryReturnDataSet : BaseInventoryIssueDataSet {
        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            result = filterMatusetrans(result);
            return result;
        }

        private ApplicationListResult filterMatusetrans(ApplicationListResult result)
        {
            var filteredResultObject =
                result.ResultObject.Where(
                    ro =>
                        double.Parse(ro.GetAttribute("qtyreturned") == null ? "0" : ro.GetAttribute("qtyreturned").ToString()) <
                        double.Parse(ro.GetAttribute("quantity").ToString()) * -1);
            result.ResultObject = filteredResultObject;
            return result;
        }

        public override string ApplicationName() {
            return "invreturn";
        }

        public override string ClientFilter() {
            return "southern_unreg,southern_reg";
        }
    }
}