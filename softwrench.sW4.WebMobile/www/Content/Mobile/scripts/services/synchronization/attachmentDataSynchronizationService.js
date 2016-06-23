(function (mobileServices, angular) {
    "use strict";

    function attachmentDataSynchronizationService($q,$log, swdbDAO, entities) {
        //#region Utils

        const matchinfFilesResolver = (matchingFiles, doclinksArray) => {

            //result will hold a list of matching files, ie, files whose hash(ids) match the ones returned from the server batch
            const attachmentsToUpdateQuery = doclinksArray.filter(syncItem => matchingFiles.some(m => m.id === syncItem.hash && m.compositionRemoteId != null)).map(item => {
                //we only need to update the attachments whose compositionRemoteId are still null on the database, since the others would mean a useless operation
                return { query: entities.Attachment.UpdateRemoteIdOfExistingAttachments, args: [String(item.compositionRemoteId), item.hash] }
            });

            const attachmentsToInsertQuery = doclinksArray.filter(syncItem => !matchingFiles.some(m => m.id === syncItem.hash)).map(item => {
                //we only need to update the attachments whose compositionRemoteId are still null on the database, since the others would mean a useless operation
                return { query: entities.Attachment.CreateNewBlankAttachments, args: [item.ownerTable, String(item.ownerId), String(item.compositionRemoteId), persistence.createUUID()] }
            });

            return $q.when(attachmentsToUpdateQuery.concat(attachmentsToInsertQuery));

        }

        //#endregion

        //#region Public methods

        function generateAttachmentsQueryArray(doclinksArray) {

            if (doclinksArray.length === 0) {
                return $q.when([]);
            }
            const querySt = entities.Attachment.NonPendingAttachments;
            //gathering the list of hashs that is coming from the server sync, and checking which ones already exist locally
            let ids = doclinksArray.filter(f => f.hash != null).map(item => item.hash).join("','");


            if (ids !== "") {
                //there's at least one file with a hash returned, perhaps we need to update it locally...
                ids = "'" + ids + "'";
                const query = { query: entities.Attachment.NonPendingAttachments, args: ids };
                return swdbDAO.executeQuery(query).then((results) => {
                    return matchinfFilesResolver(results, doclinksArray);
                });
            }
            //not a single file returned contained a hash, let's simply skip this query, since we know for sure, we won't need to update any attachment, but rather just create some
            return matchinfFilesResolver([], doclinksArray);

        }

        function downloadAttachments() {
            return swdbDAO.executeQuery(entities.Attachment.PendingAttachments).then(() => {

            });
        }

        function mergeAttachmentData(applicationName, dataEntries) {
            const log = $log.get("attachmentDataSynchronizationService#mergeAttachmentData", ["attachment", "sync"]);
            if (!dataEntries || dataEntries.length <= 0) {
                return $q.when([]);
            }



            const ids = dataEntries.map(entry => entry.remoteId).join("','");


            const queryObj = { query: entities.Attachment.ByApplicationAndIds, args: [applicationName, ids] };
            return swdbDAO.executeQuery(queryObj).then(attachments => {
                if (attachments.length === 0) {
                    log.debug("no attachments to uppload for application {0}".format(applicationName));
                    return $q.when(dataEntries);
                }
                attachments.forEach(attachment => {
                    var dataEntry = dataEntries.find(f => f.remoteId === attachment.parentId);
                    var attachmentArray = dataEntry.datamap["attachment_"];
                    if (attachmentArray == null) {
                        log.trace("skipping entry {0} since it hold no attachment".format(dataEntry.remoteId));
                        return;
                    }
//                    #offlinehash
                    var rightAttachmentDatamap = attachmentArray.find(a => a["#offlinehash"] === attachment.id);
                    rightAttachmentDatamap["newattachment"] = attachment.content;

                });
                return dataEntries;
            });


        }

        //#endregion

        //#region Service Instance
        const service = {
            generateAttachmentsQueryArray,
            downloadAttachments,
            mergeAttachmentData
        };
        return service;
        //#endregion
    }

    //#region Service registration

    mobileServices.factory("attachmentDataSynchronizationService", ["$q","$log", "swdbDAO", "offlineEntities", attachmentDataSynchronizationService]);

    //#endregion

})(mobileServices, angular);