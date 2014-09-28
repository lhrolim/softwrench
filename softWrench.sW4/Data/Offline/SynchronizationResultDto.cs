using System.Collections.Generic;
using softWrench.sW4.Data.Offline;

namespace softWrench.sW4.Data.Sync {

    public class SynchronizationResultDto {
        private readonly IList<SynchronizationApplicationData> _synchronizationData = new List<SynchronizationApplicationData>();

        public IList<SynchronizationApplicationData> SynchronizationData {
            get { return _synchronizationData; }
        }

    }
}
