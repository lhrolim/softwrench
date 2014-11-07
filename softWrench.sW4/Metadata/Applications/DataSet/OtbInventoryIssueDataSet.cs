using System;
using System.Collections.Generic;
using System.Linq;
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
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class OtbInventoryIssueDataSet : MaximoApplicationDataSet {

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject;
            LookupQuantityReturnedDetail(datamap);
            return result;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application,
            PaginatedSearchRequestDto searchDto) {
                var result = base.GetList(application, searchDto);
                LookupQuantityReturnedList(result.ResultObject);
                return result;
            }

        private void LookupQuantityReturnedList(IEnumerable<AttributeHolder> datamap ) {
            var matusetransidlist = new List<int>();
            foreach (var attributeHolder in datamap) {
                var matusetransid = attributeHolder.GetAttribute("MATUSETRANSID");
                matusetransidlist.Add(Convert.ToInt32(matusetransid));
            }

            if (!matusetransidlist.Any()) {
                return;
            }

            var user = SecurityFacade.CurrentUser();
            var commaSeparatedIds = string.Join(",", matusetransidlist);
            var query = MaxDAO.FindByNativeQuery(
                "SELECT issueid, sum(quantity) " +
                "FROM matusetrans " +
                "WHERE issueid in (" + commaSeparatedIds + ")" +
                " and siteid='" + user.SiteId + "'" +
                " group by issueid " +
                " having sum(quantity) > 0");

            if (query == null) { 
                return;
            }

            foreach (var attributeHolder in datamap) {
                var matusetransid = attributeHolder.GetAttribute("MATUSETRANSID");
                foreach (var record in query) {
                    if (record["issueid"] == matusetransid.ToString()) {
                        double qtyReturned;
                        var anyReturned = Double.TryParse(record[""], out qtyReturned);
                        if (anyReturned) {
                            attributeHolder.SetAttribute("QTYRETURNED", qtyReturned);
                        }
                        else {
                            attributeHolder.SetAttribute("QTYRETURNED", 0);
                        }
                    }
                }
            }
        }

        private void LookupQuantityReturnedDetail(AttributeHolder resultObject) {
            var user = SecurityFacade.CurrentUser();
            var issuetype = resultObject.GetAttribute("ISSUETYPE");
            var matusetransid = resultObject.GetAttribute("MATUSETRANSID");

            if (issuetype == null || issuetype.ToString() != "ISSUE" || matusetransid == null) {
                return;
            }

            var query = MaxDAO.FindByNativeQuery(
             "SELECT sum(quantity) FROM matusetrans WHERE issueid = " + matusetransid + " and siteid='" + user.SiteId + "'");

            if (query == null) {
                return;   
            }

            var queryResult = query[0].Values.FirstOrDefault();

            if (queryResult == null) {
                return;
            }

            double qtyReturned;
            var anyQtyReturned = Double.TryParse(queryResult, out qtyReturned);
            if (anyQtyReturned) {
                resultObject.SetAttribute("QTYRETURNED", qtyReturned);
            }
            else {
                resultObject.SetAttribute("QTYRETURNED", 0);
            }
            
        }

        public SearchRequestDto FilterWorkorders(AssociationPreFilterFunctionParameters parameters) {
            return AssetFilterBySiteFunction(parameters);
        }

        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null) {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
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