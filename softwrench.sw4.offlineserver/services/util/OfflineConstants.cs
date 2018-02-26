﻿namespace softwrench.sw4.offlineserver.services.util {
    public class OfflineConstants {
        
        //TODO: allow other paths other than global
        public const string AsyncBatchMinSize = "/Global/Offline/Batch/MinSize";

        public const string MaxDownloadSize = "/Global/Offline/Sync/MaxDownloadChunkSize";

        public const string MaxAssociationThreads = "/Global/Offline/Sync/MaxAssociationThreads";

        public const string SupportContactEmail = "/Global/Offline/Support/Email";

        public const string EnableAudit = "/Global/Offline/Sync/Audit";

        public const string EnableOfflineAttachments = "/Global/Offline/Sync/Attachments";

        public const string EnableParameterAuditing = "/Global/Offline/Sync/AuditParameters";

        public const string AvoidIncrementalSync = "/Global/Offline/Sync/AvoidIncremental";

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

        public const string AvoidIncrementalApp = "offline.sync.avoidincremental";


        public const string ReadonlyApplication = "mobile.application.readonly";




        /// <summary>
        ///  Use this flag to make the framework ignore a menu application for queries. Basically, the data for this app should be built exclusively at the client side.
        /// </summary>
        public const string IgnoreAsTopApp = "offline.sync.ignoretopapp";

        /// <summary>
        /// If true this will force the database operation to proceed even though the download chunk has been already overflown
        /// </summary>
        public const string SmallDataSet = "offline.sync.smalldataset";

        #endregion

    }
}

