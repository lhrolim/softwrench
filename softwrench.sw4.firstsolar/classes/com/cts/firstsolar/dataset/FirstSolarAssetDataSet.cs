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


        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var quickSearchData = searchDto.QuickSearchData;

            if (string.IsNullOrEmpty(quickSearchData)) {
                return base.GetList(application, searchDto);
            }
            if (quickSearchData.EndsWith("%") || !quickSearchData.Contains("%")) {
                //apply default search to these cases
                return base.GetList(application, searchDto);
            }

            var numberOfDashes = quickSearchData.GetNumberOfItems("-");
            if (numberOfDashes != 4) {
                return base.GetList(application, searchDto);
            }
            searchDto.AppendWhereClause("LEN(location.location) - LEN(REPLACE(location.location, '-', '')) = 4");
            return base.GetList(application, searchDto);
        }
    }
}
