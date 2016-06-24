(function (mobileServices, angular) {
    "use strict";

    function attachmentDataSynchronizationService($q,$log, swdbDAO, entities,restService) {
        //#region Utils

        const numberOfParallelDownloads = 2;


        const matchinfFilesResolver = (matchingFiles, doclinksArray) => {

            //result will hold a list of matching files, ie, files whose hash(ids) match the ones returned from the server batch
            const attachmentsToUpdateQuery = doclinksArray.filter(syncItem => matchingFiles.some(m => m.id === syncItem.hash && m.compositionRemoteId != null)).map(item => {
                //we only need to update the attachments whose compositionRemoteId are still null on the database, since the others would mean a useless operation
                return { query: entities.Attachment.UpdateRemoteIdOfExistingAttachments, args: [String(item.compositionRemoteId), String(item.docinfoid), item.hash] }
            });

            const attachmentsToInsertQuery = doclinksArray.filter(syncItem => !matchingFiles.some(m => m.id === syncItem.hash)).map(item => {
                //we only need to update the attachments whose compositionRemoteId are still null on the database, since the others would mean a useless operation
                return { query: entities.Attachment.CreateNewBlankAttachments, args: [item.ownerTable, String(item.ownerId), String(item.compositionRemoteId), String(item.docinfoid), persistence.createUUID()] }
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

        function bufferedDownload(attachmentsToDownload, originalDeferred,log) {
            const promiseDownloadBuffer = [];
            if (attachmentsToDownload.length === 0) {
                originalDeferred.resolve();
                return;
            }

            for (let i = 0; i < numberOfParallelDownloads; i++) {
                const promiseObj = attachmentsToDownload.shift();
                promiseDownloadBuffer.push(promiseObj);
            }
            const promisesToExecute = promiseDownloadBuffer.filter(f=> f != null);
            if (promisesToExecute.length === 0) {
                originalDeferred.resolve();
                return;
            }

            $q.all(promisesToExecute.map(p => p.promise)).then(function (results) {
                const updateQueriesObject = [];
                for (let i = 0; i < promiseDownloadBuffer.length; i++) {
                    const result = results[i];
                    if (!result) {
                        log.warn("server returned with a null file result... saving a error file to prevent downloading");
                        const queryObj = { query: entities.Attachment.UpdatePendingAttachment, args: ["error", "error", promiseDownloadBuffer[i].id] }
                        updateQueriesObject.push(queryObj);
                    } else {
                        log.warn("server returned with a null file result... will");
                        const data = result.data;
                        const queryObj = { query: entities.Attachment.UpdatePendingAttachment, args: [data.content, data.mimeType, promiseDownloadBuffer[i].id] }
                        updateQueriesObject.push(queryObj);
                    }
                    
                }
                swdbDAO.executeQueries(updateQueriesObject).then(bufferedDownload(attachmentsToDownload, originalDeferred,log));
            });

        }

        function downloadAttachments() {
            const log = $log.get("attachmentDataSynchronizationService#downloadAttachments", ["attachment", "sync","download"]);

            return swdbDAO.executeQuery(entities.Attachment.PendingAttachments).then((attachmentsToDownload) => {
                if (attachmentsToDownload.length === 0) {
                    log.debug("no attachments to download, resuming");
                    return null;
                }

                var deferred = $q.defer();

                var fullPromiseBuffer = [];
                var length = attachmentsToDownload.length;

                attachmentsToDownload.forEach(value => {
                    const attachment = value;
                    const promise = restService.get("OfflineAttachment", "DownloadBase64", { id: attachment.docinfoRemoteId });
                    fullPromiseBuffer.push({ id: attachment.id, promise: promise });
                });
                bufferedDownload(fullPromiseBuffer, deferred,log);

                return deferred.promise;


            });
        }

        /**
         * Called just before the sync to insert the base64 data into the parent datamaps. Amongst other possible reasons, we prevent this data on the datamap so that the global search doesn't get impacted.
         * 
         * @param {} applicationName the name of the application being synced. On a sync operation we might call this method each time per application
         * @param {} dataEntries the parent entries in which the attachments should be inserted into
         * @returns {Promise} the new dataentries containing the base64 data
         */
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

    mobileServices.factory("attachmentDataSynchronizationService", ["$q", "$log", "swdbDAO", "offlineEntities", "offlineRestService", attachmentDataSynchronizationService]);

    //#endregion

})(mobileServices, angular);