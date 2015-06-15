using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class BaseInventoryReturnDataSet : BaseInventoryIssueDataSet {
        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
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
            return "southern_unreg";
        }
    }
}