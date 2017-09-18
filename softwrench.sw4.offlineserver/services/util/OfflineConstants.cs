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

        public const string SupportContactEmail = "/Global/Offline/Support/Email";

        public const string EnableAudit = "/Global/Offline/Sync/Audit";

        public const string AllowedClientVersions = "/Global/Offline/Sync/AllowedVersions";


        #region metadataproperties
        /// <summary>
        /// If true, this application will try to bring database entries on top of the cached entries (i.e only the cached entries will be returned).
        /// 
        /// If no cache entries are found, however, the database shall still be hit
        /// 
        /// </summary>
        public const string CheckDatabaseAfterCache = "offline.sync.checkdatabase";

        public const string AvoidCaching = "offline.sync.avoidcaching";

        /// <summary>
        /// If true this will force the database operation to proceed even though the download chunk has been already overflown
        /// </summary>
        public const string SmallDataSet = "offline.sync.smalldataset";

        #endregion

    }
}
