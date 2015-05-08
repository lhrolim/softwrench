using System.Collections.Generic;

namespace softwrench.sw4.offlineserver.dto {

    public class SynchronizationResultDto {

        public IList<SynchronizationApplicationResultData> TopApplicationData { get; set; }

        public IList<SynchronizationApplicationResultData> CompositionData { get; set; }

        public void AddTopApplicationData(SynchronizationApplicationResultData newResultData) {
            if (TopApplicationData == null) {
                TopApplicationData = new List<SynchronizationApplicationResultData>();
            }
            TopApplicationData.Add(newResultData);
        }

        public void AddCompositionData(SynchronizationApplicationResultData newResultData) {
            if (CompositionData == null) {
                CompositionData = new List<SynchronizationApplicationResultData>();
            }
            CompositionData.Add(newResultData);
        }


    }
}
