using System.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {
    public class WoBatchDataSet : SWDBApplicationDataset {



        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            foreach (var item in result.ResultObject) {
                var itemIds = item.GetAttribute("itemids") as string;
                item.SetAttribute("#numberofitems", itemIds == null ? 0 : itemIds.Count(f => f == ',')+1);
            }
            return result;
        }
        public override string ApplicationName() {
            return "_wobatch";
        }


    }
}
