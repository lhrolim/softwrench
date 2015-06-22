using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Applications.DataSet;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Inventory {
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