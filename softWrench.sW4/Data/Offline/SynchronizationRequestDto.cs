using System.Collections.Generic;

namespace softWrench.sW4.Data.Sync {
    public class SynchronizationRequestDto {
        public List<ApplicationSyncData> Applications { get; set; }

        public class ApplicationSyncData {
            public string appName { get; set; }
            public bool FetchMetadata { get; set; }

            public ApplicationSyncData() {
                FetchMetadata = false;
            }

            //            public string schemaId { get; set; }
            public string UpperLimitRowstamp { get; set; }
            public string LowerLimitRowstamp { get; set; }


        }
    }
}
