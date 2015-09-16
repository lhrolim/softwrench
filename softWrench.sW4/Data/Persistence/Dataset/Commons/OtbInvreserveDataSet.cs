using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    class OtbInvreserveDataSet : MaximoApplicationDataSet {

        public IEnumerable<IAssociationOption> GetAvailableBins(OptionFieldProviderParameters parameters) {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            var orgid = parameters.OriginalEntity.GetAttribute("orgid");
            var location = parameters.OriginalEntity.GetAttribute("location");
            var lotnum = parameters.OriginalEntity.GetAttribute("lotnum");

            var query = string.Format("select distinct binnum " +
                                      "from invbalances " +
                                      "where itemnum = '{0}' and " +
                                            "siteid = '{1}' and " +
                                            "orgid = '{2}' and " +
                                            "location = '{3}' and " +
                                            "curbal > 0 and " +
                                            "binnum is not null",
                                      itemnum, siteid, orgid, location, lotnum);

            var result = MaxDAO.FindByNativeQuery(query, null);
            var availableLocations = new List<IAssociationOption>();
            if (result.Any()) {
                foreach (var record in result) {
                    var recordAssociationOption = new AssociationOption(record["binnum"], record["binnum"]);
                    if (!availableLocations.Contains(recordAssociationOption)) {
                        availableLocations.Add(recordAssociationOption);
                    }
                }
            }

            return availableLocations.AsEnumerable();
        }

        public IEnumerable<IAssociationOption> GetAvailableLots(OptionFieldProviderParameters parameters) {
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var itemnum = parameters.OriginalEntity.GetAttribute("itemnum");
            var orgid = parameters.OriginalEntity.GetAttribute("orgid");
            var location = parameters.OriginalEntity.GetAttribute("location");
            var binnum = parameters.OriginalEntity.GetAttribute("#frombin");

            var query = string.Format("select distinct lotnum " +
                                      "from invbalances " +
                                      "where itemnum = '{0}' and " +
                                            "siteid = '{1}' and " +
                                            "orgid = '{2}' and " +
                                            "location = '{3}' and " +
                                            "binnum = '{4}' and " +
                                            "curbal > 0 and " +
                                            "binnum is not null and " +
                                            "lotnum is not null",
                                      itemnum, siteid, orgid, location, binnum);

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

        public override ApplicationListResult GetList(ApplicationMetadata application,
            PaginatedSearchRequestDto searchDto) {
            var result = base.GetList(application, searchDto);
            return result;
        }

        public SearchRequestDto FilterBins(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Clear();
            filter.ProjectionFields.Add(new ProjectionField("binnum", "COALESCE(invbalances.binnum, '')"));
            filter.ProjectionFields.Add(new ProjectionField("curbal", "invbalances.curbal"));
            filter.ProjectionFields.Add(new ProjectionField("lotnum", "invbalances.lotnum"));
            if (!filter.SearchParams.Contains("itemnum")) {
                filter.AppendSearchParam("itemnum");
            }
            if (!filter.SearchValues.Contains(parameters.OriginalEntity.GetAttribute("itemnum").ToString())) {
                filter.AppendSearchValue(parameters.OriginalEntity.GetAttribute("itemnum").ToString());
            }
            filter.SearchSort = "invbalances.binnum,invbalances.lotnum";
            filter.SearchAscending = true;

            return filter;
        }

        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var validAssetStatus = new List<string> { "NOT READY", "OPERATING", "ACTIVE" };
            filter.AppendSearchEntry("STATUS", validAssetStatus);
            return filter;
        }

        public override string ApplicationName() {
            return "reservedMaterials";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}