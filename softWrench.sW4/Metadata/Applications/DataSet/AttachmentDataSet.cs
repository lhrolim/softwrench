using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.WS.Commons;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    class AttachmentDataSet : MaximoApplicationDataSet {

        private static readonly AttachmentHandler _attachmentHandler = new AttachmentHandler();


//        public override SynchronizationApplicationData Sync(ApplicationMetadata applicationMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData) {
//            return Engine().
//                Sync(applicationMetadata, applicationSyncData,AttachmentSyncDelegate);
//        }

        public override string ApplicationName() {
            return "attachment";
        }

//        private static SyncItemHandler.SyncedItemHandlerDelegate AttachmentSyncDelegate =
//            delegate(KeyValuePair<object, DataMap> item, ApplicationMetadata metadata) {
//                var docinfoURL = (string)item.Value.GetAttribute("docinfo_.urlname", false, true);
//                var maximoURL = _attachmentHandler.GetFileUrl(docinfoURL);
//                //TODO: make data field name, somehow,customizable
//                if (maximoURL != null) {
//                    item.Value.Fields.Add("maximoURL", maximoURL);
//                }
//            };
    }
}
