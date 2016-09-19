using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.amex.classes.com.cts.amex.dataset {
    public class AmexAssetDataSet : BaseAssetDataSet {
        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            searchDto.SearchSort = "ASSET.MANUFACTURER, ASSET.ITEMNUM, ASSET.DESCRIPTION";
            searchDto.SearchAscending = true;
            var result = await base.GetList(application, searchDto);
            return result;
        }


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
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
