using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagOfferingDataSet : HapagBaseApplicationDataSet {

        protected override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            return result;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var baseDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = baseDetail.ResultObject;
            resultObject.SetAttribute("minstartdate", DateUtil.BeginOfDay(DateTime.Now.AddBusinessDays(3)));
            return baseDetail;
        }


        public override string ApplicationName() {
            return "offering";
        }


    }
}
