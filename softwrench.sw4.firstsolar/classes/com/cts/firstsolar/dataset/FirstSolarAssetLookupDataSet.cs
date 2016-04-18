using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarAssetLookupDataSet : BaseAssetDataSet {
        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            return base.GetList(application, searchDto);
        }


        public override string ApplicationName() {
            return "asset";
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        public override string SchemaId() {
            return "assetLookupList";
        }
    }
}
