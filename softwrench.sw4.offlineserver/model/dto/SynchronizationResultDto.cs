using System.Collections.Generic;
using System.Linq;

namespace softwrench.sw4.offlineserver.model.dto {

    public class SynchronizationResultDto {

        public SynchronizationResultDto() {
            CompositionData = new List<SynchronizationApplicationResultData>();
            TopApplicationData = new List<SynchronizationApplicationResultData>();
        }

        /// <summary>
        /// used because at the initial sync we might need to override some custom services at the client side
        /// </summary>
        public string ClientName { get; set; }

        public IList<SynchronizationApplicationResultData> TopApplicationData { get; set; }

        public IList<SynchronizationApplicationResultData> CompositionData { get; set; }

        public int AttachmentCount { get; set; }

        public IDictionary<string, object> UserProperties { get; set; }

        public bool FacilitiesUpdated { get; set; }

        public void AddTopApplicationData(SynchronizationApplicationResultData newResultData) {
            lock (string.Intern("lock:addtopdata")) {
                if (TopApplicationData == null) {
                    TopApplicationData = new List<SynchronizationApplicationResultData>();
                }
                TopApplicationData.Add(newResultData);
            }
        }

        public void AddCompositionData(SynchronizationApplicationResultData newResultData) {
            lock (string.Intern("lock:addcompdata")) {
                if (CompositionData == null) {
                    CompositionData = new List<SynchronizationApplicationResultData>();
                }
                CompositionData.Add(newResultData);
            }
        }

        public bool IsEmpty {
            get { return TopApplicationData.Union(CompositionData).All(data => data.IsEmpty); }
        }

    }
}
