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
    class OtbInventoryIssueDataSet : BaseInventoryIssueDataSet {

        public override SearchRequestDto FilterBins(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Add(new ProjectionField("binnum", "COALESCE(invbalances.binnum, '')"));
            filter.SearchSort = "invbalances.binnum,invbalances.lotnum";
            filter.SearchAscending = true;
            
            return filter;
        }

        public override string ApplicationName() {
            return "invissue";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}