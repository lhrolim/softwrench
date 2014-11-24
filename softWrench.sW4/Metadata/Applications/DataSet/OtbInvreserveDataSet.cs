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

            var query = string.Format("select binnum " +
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

            foreach (var record in result)
            {
                availableLocations.Add(new AssociationOption(record["binnum"], record["binnum"]));
            }

            return availableLocations.AsEnumerable();
        }

        public override string ApplicationName() {
            return "reservedMaterials";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}