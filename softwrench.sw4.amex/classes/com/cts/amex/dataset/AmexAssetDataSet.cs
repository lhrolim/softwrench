using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.amex.classes.com.cts.amex.dataset {
    public class AmexAssetDataSet : MaximoApplicationDataSet {
        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            searchDto.SearchSort = "ASSET.MANUFACTURER, ASSET.ITEMNUM, ASSET.DESCRIPTION";
            searchDto.SearchAscending = true;
            var result = base.GetList(application, searchDto);
            return result;
        }


        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            result.ResultObject.SetAttribute("#recdate", DateTime.Now);
            return result;
        }

        public override string ApplicationName() {
            return "asset";
        }

        public override string ClientFilter() {
            return "amex";
        }
    }
}
