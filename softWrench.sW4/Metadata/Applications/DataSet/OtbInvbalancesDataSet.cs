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

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class OtbInvbalancesDataSet : MaximoApplicationDataSet {

        public IEnumerable<AssociationOption> GetKitOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT distinct iskit, case when iskit = 1 then 'Yes' else 'No' end as label FROM item").ToList();
            return rows.Select(row => new AssociationOption(row["iskit"], row["label"])).ToList();
        }

        public IEnumerable<AssociationOption> GetRotatingOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT distinct rotating, case when rotating = 1 then 'Yes' else 'No' end as label FROM item").ToList();
            return rows.Select(row => new AssociationOption(row["rotating"], row["label"])).ToList();
        }

        public override string ApplicationName() {
            return "invbalances";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}