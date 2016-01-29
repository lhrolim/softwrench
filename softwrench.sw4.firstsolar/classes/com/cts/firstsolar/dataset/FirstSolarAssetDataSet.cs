using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    class FirstSolarAssetDataSet : MaximoApplicationDataSet {

        public override string ApplicationName() {
            return "asset";
        }

        public override string ClientFilter() {
            return "firstsolar";
        }

        private readonly FirstSolarPCSLocationHandler _pcsLocationHandler;

        public FirstSolarAssetDataSet(FirstSolarPCSLocationHandler pcsLocationHandler) {
            _pcsLocationHandler = pcsLocationHandler;
        }


        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var quickSearchData = searchDto.QuickSearchData;

            if (string.IsNullOrEmpty(quickSearchData)) {
                return base.GetList(application, searchDto);
            }

            var query = _pcsLocationHandler.DoGetPCSQuery(quickSearchData, "asset");
            if (query != null) {
                searchDto.AppendWhereClause(query);
            }

            return base.GetList(application, searchDto);
        }
    }
}
