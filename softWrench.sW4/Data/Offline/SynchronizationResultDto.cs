using System.Collections.Generic;
using System.Security.RightsManagement;
using DocumentFormat.OpenXml.Drawing.Charts;
using softWrench.sW4.Data.Offline;

namespace softWrench.sW4.Data.Sync {

    public class SynchronizationResultDto {

        public IList<SynchronizationApplicationData> TopApplicationData { get; set; }

        public IList<SynchronizationApplicationData> CompositionData { get; set; }

        public void AddTopApplicationData(SynchronizationApplicationData newData) {
            if (TopApplicationData == null) {
                TopApplicationData = new List<SynchronizationApplicationData>();
            }
            TopApplicationData.Add(newData);
        }

        public void AddCompositionData(SynchronizationApplicationData newData) {
            if (CompositionData == null) {
                CompositionData = new List<SynchronizationApplicationData>();
            }
            CompositionData.Add(newData);
        }


    }
}
