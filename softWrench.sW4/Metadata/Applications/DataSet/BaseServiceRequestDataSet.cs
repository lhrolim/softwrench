using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseServiceRequestDataSet : BaseApplicationDataSet {

        public IEnumerable<IAssociationOption> FilterAvailableStatus(DataMap currentSR, IEnumerable<AssociationOption> loadedAssociations) {
            var currentStatus = (string)currentSR.GetAttribute("status");
            var filterAvailableStatus = loadedAssociations as AssociationOption[] ?? loadedAssociations.ToArray();
            if (currentStatus == null) {
                return new List<AssociationOption> { filterAvailableStatus.First(l => l.Value == "OPEN") };
            }
            var currentOption = filterAvailableStatus.FirstOrDefault(l => l.Value == currentStatus);
            if (currentOption == null) {
                return filterAvailableStatus;
            }

            if (currentStatus == "APPR" || currentStatus == "WAPPR") {
                return filterAvailableStatus;
            }
            if (currentStatus == "COMP") {
                return new List<AssociationOption> { currentOption, filterAvailableStatus.First(l => l.Value == "CLOSE") };
            }
            return new List<AssociationOption> { currentOption };
        }

        public override string ApplicationName() {
            return "servicerequest";
        }
    }
}
