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
                String.Format("SELECT issueid, sum(quantity) FROM matusetrans WHERE issueid in ({0}) and siteid='{1}' group by issueid having sum(quantity) > 0",commaSeparatedIds,user.SiteId));

            if (query == null) { 
                return;
            }

            //For each matusetrans record in the datamap and each result in the above query,
            //checks to see if there was a match between the attribute holder's matusetransid and the query's issueid.
            //If a match is found, the summation is set as the qtyReturned value.
            //This is needed because Maximo itself does not provide a field to sum the quantity field for all
            //items returned to an issue. Instead, they perform this exact query.
            //Quantity returned is set to 0 if no match is found (i.e. no items have been returned to the issue)
            foreach (var attributeHolder in datamap) {
                var matusetransid = attributeHolder.GetAttribute("MATUSETRANSID");
                foreach (var record in query) {
                    if (record["issueid"] == matusetransid.ToString()) {
                        double qtyReturned;
                        var anyReturned = Double.TryParse(record[""], out qtyReturned);
                        attributeHolder.SetAttribute("QTYRETURNED", anyReturned ? qtyReturned : 0);
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

            var query = MaxDAO.FindSingleByNativeQuery<object>(
             String.Format("SELECT sum(quantity) FROM matusetrans WHERE issueid = {0} and siteid='{1}'", matusetransid, user.SiteId));

            if (query == null) {
                return;   
            }

            double qtyReturned;
            var anyQtyReturned = Double.TryParse(query.ToString(), out qtyReturned);
            resultObject.SetAttribute("QTYRETURNED", anyQtyReturned ? qtyReturned : 0);
        }

        public SearchRequestDto FilterWorkorders(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.Attributes["siteid"];
            if (siteid != null) {
                filter.AppendSearchEntry("workorder.siteid", siteid.ToString().ToUpper());
                var validWorkOrderStatus = new List<string> { "APPR", "WMAT", "WSCH", "WORKING" };
                filter.AppendSearchEntry("STATUS", validWorkOrderStatus);
            }
            return filter;
        }

        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.Attributes["siteid"];
            if (siteid != null) {
                filter.AppendSearchEntry("asset.siteid", siteid.ToString().ToUpper());
            }
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