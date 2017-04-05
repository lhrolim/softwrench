using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.offlineserver.services.util {
    public class OfflineConstants {
        
        //TODO: allow other paths other than global
        public const string AsyncBatchMinSize = "/Global/Offline/Batch/MinSize";

        public const string MaxDownloadSize = "/Global/Offline/Sync/MaxDownloadChunkSize";

        public const string MaxAssociationThreads = "/Global/Offline/Sync/MaxAssociationThreads";

        public const string SupportContactEmail = "/Offline/Support/Email";

    }
}
