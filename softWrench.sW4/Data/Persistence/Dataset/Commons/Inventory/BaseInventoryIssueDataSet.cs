using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {
    class BaseInventoryIssueDataSet : MaximoApplicationDataSet {

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject;
            LookupQuantityReturnedDetail(datamap);
            return result;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application,
            PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            LookupQuantityReturnedList(result.ResultObject);
            return result;
        }

        private void LookupQuantityReturnedList(IEnumerable<AttributeHolder> datamap) {
            var matusetransidlist = datamap.Select(row => row["matusetransid"]).ToList();

            if (!matusetransidlist.Any()) {
                return;
            }

            var commaSeparatedIds = String.Join(",", matusetransidlist);
            var query = MaxDAO.FindByNativeQuery(
                String.Format("SELECT issueid, sum(quantity) AS returned FROM matusetrans WHERE issueid in ({0}) group by issueid having sum(quantity) > 0", commaSeparatedIds));

            //For each matusetrans record in the query result and each result in datamap,
            //checks to see if there was a match between the attribute holder's matusetransid and the query's issueid.
            //If a match is found, the summation is set as the qtyReturned value.
            //This is needed because Maximo itself does not provide a field to sum the quantity field for all
            //items returned to an issue. Instead, they perform this exact query.
            //Quantity returned is set to 0 if no match is found (i.e. no items have been returned to the issue)

            // CC: Worst case scenario, it will equal the number of records in the datamap. 
            // CC: Note: I did not set remaining records to zero because of Luiz changes in the grid.  
            if (query.Any()) {
                foreach (var entry in query) {
                    // CC: This is now safe because we are guarantee a record before we couldn't determine if we could find a matching record
                    // CC: matusetransid is case sensitive... maybe change the dictionary to case-insensitive.   
                    var datamapRecord = (from record in datamap
                                         where record["matusetransid"].ToString() == entry["issueid"].ToString()
                                         select record).SingleOrDefault();

                    datamapRecord.SetAttribute("QTYRETURNED", Double.Parse(entry["returned"]));
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
            var siteid = parameters.OriginalEntity["siteid"];
            if (siteid != null) {
                filter.AppendSearchEntry("workorder.siteid", siteid.ToString().ToUpper());
                var validWorkOrderStatus = new List<string> { "APPR", "WMAT", "WSCH", "WORKING" };
                filter.AppendSearchEntry("STATUS", validWorkOrderStatus);
            }
            return filter;
        }

        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity["siteid"];
            if (siteid != null) {
                filter.AppendSearchEntry("asset.siteid", siteid.ToString().ToUpper());
            }
            return filter;
        }

        public virtual SearchRequestDto FilterBins(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
//            filter.ProjectionFields.Add(new ProjectionField("binnum", "COALESCE(invbalances.binnum, '')"));
            filter.SearchSort = "invbalances.binnum,invbalances.lotnum";
            filter.SearchAscending = true;

            return filter;
        }

        public virtual SearchRequestDto FilterStoreRooms(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            if (string.IsNullOrEmpty((string)parameters.OriginalEntity.GetAttribute("itemnum"))) {
                //for non batch mode, the item gets selected after the storeroom, so any valid storeroom is ok
                return filter;
            }
            filter.AppendWhereClauseFormat("(select CAST(SUM(COALESCE(curbal, 0)) AS INT) from invbalances where invbalances.itemnum = '{0}' " +
                                           "and invbalances.siteid = location.siteid and invbalances.location = location.location) > 0", parameters.OriginalEntity.GetAttribute("itemnum"));
            return filter;
        }

        public IEnumerable<IAssociationOption> GetAvailableLots(OptionFieldProviderParameters parameters) {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            var location = parameters.OriginalEntity.GetAttribute("storeloc");
            var binnum = parameters.OriginalEntity.GetAttribute("binnum");

            var query = string.Format("select distinct lotnum " +
                                      "from invbalances " +
                                      "where itemnum = '{0}' and " +
                                            "siteid = '{1}' and " +
                                            "location = '{2}' and " +
                                            "binnum = '{3}' and " +
                                            "curbal > 0 and " +
                                            "binnum is not null and " +
                                            "lotnum is not null",
                                      itemnum, siteid, location, binnum);

            var result = MaxDAO.FindByNativeQuery(query, null);
            var availableLocations = new List<IAssociationOption>();
            if (result.Any()) {
                foreach (var record in result) {
                    var recordAssociationOption = new AssociationOption(record["lotnum"], record["lotnum"]);
                    if (!availableLocations.Contains(recordAssociationOption)) {
                        availableLocations.Add(recordAssociationOption);
                    }
                }
            }

            return availableLocations.AsEnumerable();
        }

        public override string ApplicationName() {
            return "invissue,keyinvissue";
        }

        public override string ClientFilter() {
            return "bering,pae,southern_unreg,southern_reg";
        }
    }
}