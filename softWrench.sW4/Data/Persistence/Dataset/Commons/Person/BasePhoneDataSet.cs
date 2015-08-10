using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Person {
    public class BasePhoneDataSet : MaximoApplicationDataSet {

        public IEnumerable<AssociationOption> GetPhoneTypeOptions(OptionFieldProviderParameters parameters) {
            var rows = MaxDAO.FindByNativeQuery(
                "SELECT value, description as label FROM alndomain WHERE domainid = 'PHONETYPE'").ToList();
            return rows.Select(row => new AssociationOption(row["value"], row["label"])).ToList();
        } 

        public override string ApplicationName() {
            return "phone";
        }

        public override string ClientFilter() {
            return "otb";
        }
    }
}
