using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {

    class BaseInvuseDataSet : MaximoApplicationDataSet {

        public SearchRequestDto FilterToStoreLoc(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            const string type = "STOREROOM";
            filter.AppendSearchEntry("location.type", type.ToUpper());

            var siteid = parameters.OriginalEntity.GetAttribute("siteid");

            if (siteid != null) {
                filter.AppendSearchEntry("location.siteid", siteid.ToString().ToUpper());
            }

            return filter;
        }

        public SearchRequestDto FilterFromStoreLoc(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            filter.AppendSearchEntry("location.siteid", siteid.ToString().ToUpper());
            filter.SearchSort = "location.location asc";

            filter.AppendWhereClauseFormat("(select CAST(SUM(COALESCE(curbal, 0)) AS INT) from invbalances where invbalances.itemnum = '{0}' " +
                                       "and invbalances.siteid = location.siteid and invbalances.location = location.location) > 0", parameters.OriginalEntity.GetAttribute("itemnum"));
            return filter;

        }

        public SearchRequestDto FilterItem(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.SearchSort = "inventory.itemnum";

            return filter;
        }

        public SearchRequestDto FilterFromBin(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("fromstoreloc");
            
            if (siteid == null || location == null) {
                return filter;
            }
            filter.AppendSearchEntry("invbalances.itemnum", itemnum.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.siteid", siteid.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Clear();
            filter.ProjectionFields.Add(new ProjectionField("binnum", "COALESCE(invbalances.binnum, '')"));
            filter.ProjectionFields.Add(new ProjectionField("curbal", "invbalances.curbal"));
            filter.ProjectionFields.Add(new ProjectionField("lotnum", "invbalances.lotnum"));
            filter.SearchSort = "invbalances.binnum";
            filter.SearchAscending = true;

            return filter;
        }

        public SearchRequestDto FilterToBin(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("tostoreloc");
            if (siteid == null || location == null) {
                return filter;
            }
            filter.AppendSearchEntry("invbalances.itemnum", itemnum.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.siteid", siteid.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Clear();
            filter.ProjectionFields.Add(new ProjectionField("binnum", "COALESCE(invbalances.binnum, '')"));
            filter.ProjectionFields.Add(new ProjectionField("curbal", "invbalances.curbal"));
            filter.ProjectionFields.Add(new ProjectionField("lotnum", "invbalances.lotnum"));
            filter.SearchSort = "invbalances.binnum";
            filter.SearchAscending = true;

            return filter;
        }

        //public SearchRequestDto FilterBin(SearchRequestDto filter, object siteid, object location) {
        //    if (siteid == null || location == null) {
        //        return filter;
        //    }
        //    filter.AppendSearchEntry("inventory_.siteid", siteid.ToString().ToUpper());
        //    filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
        //    filter.SearchAscending = true;
        //    return filter;
        //}

        public IEnumerable<IAssociationOption> GetAvailableLots(OptionFieldProviderParameters parameters) {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var itemnum = parameters.OriginalEntity.GetAttribute("invuseline_.itemnum");
            var location = parameters.OriginalEntity.GetAttribute("fromstoreloc");
            var binnum = parameters.OriginalEntity.GetAttribute("invuseline_.frombin");

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
            return "invuse";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}