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

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class OtbInventoryIssueDataSet : BaseInventoryIssueDataSet {

            var filter = parameters.BASEDto;
            var siteid = parameters.OriginalEntity.GetAttribute("siteid");
            var location = parameters.OriginalEntity.GetAttribute("storeloc");
            if (siteid == null || location == null) {
                return filter;
            }
            filter.AppendSearchEntry("invbalances.siteid", siteid.ToString().ToUpper());
            filter.AppendSearchEntry("invbalances.location", location.ToString().ToUpper());
            filter.AppendWhereClauseFormat("invbalances.stagingbin = 0");
            filter.ProjectionFields.Clear();
            filter.ProjectionFields.Add(new ProjectionField("binnum", "ISNULL(invbalances.binnum, '')"));
            filter.ProjectionFields.Add(new ProjectionField("curbal", "invbalances.curbal"));
            filter.ProjectionFields.Add(new ProjectionField("lotnum", "invbalances.lotnum"));
            filter.SearchSort = "invbalances.binnum,invbalances.lotnum";
            filter.SearchAscending = true;
            
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

        public override string ApplicationName() {
            return "invissue";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}