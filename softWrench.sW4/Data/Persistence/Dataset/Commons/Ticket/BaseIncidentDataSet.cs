using System.Collections.Generic;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {
    class BaseIncidentDataSet : BaseTicketDataSet {


        public IEnumerable<IAssociationOption> GetIncidentClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "INCIDENT");
        }

        public IEnumerable<IAssociationOption> GetAssetClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "ASSET");
        }

        public override string ApplicationName() {
            return "incident";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
