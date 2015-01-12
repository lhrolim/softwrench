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
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {
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
            if (result != null) {
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
            if (result != null) {
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
            LookupQuantityAvailableList(result.ResultObject);
            return result;
        }

        private void LookupQuantityAvailableList(IEnumerable<AttributeHolder> datamap) {
            var itemnumList = datamap.Select(attributeHolder => attributeHolder.GetAttribute("ITEMNUM")).ToList();
            // If the list is empty return
            if (!itemnumList.Any()) {
                return;
            }
            var commaSeparatedItemnums = String.Join(",", itemnumList);
            commaSeparatedItemnums = "'" + commaSeparatedItemnums.Replace(",", "','") + "'";
            var query = String.Format("SELECT itemnum, siteid, location, CAST(sum(curbal) AS INT) as curbal " +
                                      "FROM invbalances " +
                                      "WHERE itemnum in ({0}) " +
                                      "GROUP BY itemnum, siteid, location " +
                                      "HAVING sum(curbal) > 0",
                                      commaSeparatedItemnums);
            var resultSet = MaxDAO.FindByNativeQuery(query);
            // If no items were found return
            if (resultSet == null) {
                return;
            }

            // For each invreserve record in the datamap,
            // find the corresponding inventory quantity from
            // the query above and set the available quantity.
            foreach (var attributeHolder in datamap) {
                var itemnum = attributeHolder.GetAttribute("ITEMNUM");
                var siteid = attributeHolder.GetAttribute("SITEID");
                var location = attributeHolder.GetAttribute("LOCATION") ?? "";
                var result = resultSet.FirstOrDefault(res => res["ITEMNUM"] == itemnum.ToString() &&
                                                             res["SITEID"] == siteid.ToString() &&
                                                             res["LOCATION"] == location.ToString());
                // Set a default of zero in case there are no invbalace records
                var qtyAvailable = "0";
                if (result != null) {
                    qtyAvailable = result["curbal"];
                }
                attributeHolder.SetAttribute("AVAILABLEQUANTITY", qtyAvailable);
            }
        }

        public SearchRequestDto FilterBins(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Clear();
            filter.ProjectionFields.Add(new ProjectionField("binnum", "ISNULL(invbalances.binnum, '')"));
            filter.ProjectionFields.Add(new ProjectionField("curbal", "invbalances.curbal"));
            filter.ProjectionFields.Add(new ProjectionField("lotnum", "invbalances.lotnum"));
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