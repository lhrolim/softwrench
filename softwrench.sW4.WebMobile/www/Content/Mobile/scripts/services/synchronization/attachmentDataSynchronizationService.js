(function (mobileServices, angular) {
    "use strict";

    function attachmentDataSynchronizationService($q, $log, swdbDAO, entities, restService) {
        //#region Utils

        const numberOfParallelDownloads = 2;


        const matchinfFilesResolver = (matchingFiles, doclinksArray, log) => {

            

            const updateFiltercondition = syncItem => matchingFiles.some(m => m.id === syncItem.hash && (m.compositionRemoteId == null || m.docinfoRemoteId == null));

            //result will hold a list of matching files, ie, files whose hash(ids) match the ones returned from the server batch
            const attachmentsToUpdateQuery = doclinksArray.filter(updateFiltercondition).map(item => {
                //we only need to update the attachments whose compositionRemoteId are still null on the database, since the others would mean a useless operation
                return { query: entities.Attachment.UpdateRemoteIdOfExistingAttachments, args: [String(item.compositionRemoteId), String(item.docinfoid), item.hash] }
            });

            //using == instead of === to avoid string/numeric breakings
            const attachmentsToInsertQuery = doclinksArray.filter(syncItem => !matchingFiles.some(m => (m.docinfoRemoteId == syncItem.docinfoid)))
                .filter(syncItem => !matchingFiles.some(m => m.id === syncItem.hash && (m.compositionRemoteId == null || m.docinfoRemoteId == null)))
                .map(item => {
                //creating the attachments which could not be found for a given composition, excluding the ones that just got updated on the previous condition
                return { query: entities.Attachment.CreateNewBlankAttachments, args: [item.ownerTable, String(item.ownerId), String(item.compositionRemoteId), String(item.docinfoid), persistence.createUUID()] }
            });
            const attachmentQueries = attachmentsToUpdateQuery.concat(attachmentsToInsertQuery);

            if (attachmentQueries.length === 0) {
                log.debug("no attachments to be inserted/updated");
            }

            if (attachmentsToUpdateQuery.length !== 0) {
                log.debug(`${attachmentsToUpdateQuery.length} locally created attachments will get updated`);
            }

            if (attachmentsToInsertQuery.length !== 0) {
                log.debug(`${attachmentsToInsertQuery.length} attachments will get created locally`);
            }

            return $q.when(attachmentQueries);
        }

        //#endregion

        //#region Public methods
        /**
         *  Generates the queries for 
         *  1) inserting into Attachments ones that were not created locally (either on other devices or on the online mode)
         *  2) updating the remoteids for the attachments that were created locally on this particular device
         * 
         * @param {} doclinksArray 
         * @returns {} 
         */
        function generateAttachmentsQueryArray(doclinksArray) {

            if (doclinksArray.length === 0) {
                return $q.when([]);
            }
            const querySt = entities.Attachment.NonPendingAttachments;
            //gathering the list of hashs that is coming from the server sync, and checking which ones already exist locally
            //hashs would only exist for files that got created on offline devices!!

            let ids = doclinksArray.filter(f => f.hash != null).map(item => item.hash).join("','");

            let docinfoRemoteId = "'" + doclinksArray.map(item => item.docinfoid).join("','") + "'";

            const log = $log.get("attachmentDataSynchronizationService#generateAttachmentsQueryArray", ["attachment", "sync", "download"]);

            if (ids !== "") {
                ids = "'" + ids + "'";
            } 

            log.debug(`determining which attachments should be downloaded amongst ${ids} and remoteids ${docinfoRemoteId} `);
            return swdbDAO.executeQuery(entities.Attachment.NonPendingAttachments.format(ids, docinfoRemoteId)).then((results) => {
                return matchinfFilesResolver(results, doclinksArray, log);
            });
        }

        function bufferedDownload(attachmentsToDownload, originalDeferred, log) {
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
                for (let i = 0; i < promisesToExecute.length; i++) {
                    const result = results[i];
                    const localFileId = promisesToExecute[i].id;
                    if (!result || !result.data) {
                        log.warn("server returned with a null file result... saving a error file to prevent downloading");
                        const queryObj = { query: entities.Attachment.UpdatePendingAttachment, args: ["error", "error", localFileId] }
                        updateQueriesObject.push(queryObj);
                    } else {
                        log.info("storing attachment for localid {0} ".format(localFileId));
                        const data = result.data;
                        const queryObj = { query: entities.Attachment.UpdatePendingAttachment, args: [data.content, data.mimeType, localFileId] }
                        updateQueriesObject.push(queryObj);
                    }

                }
                swdbDAO.executeQueries(updateQueriesObject).then(bufferedDownload(attachmentsToDownload, originalDeferred, log));
            });

        }

        function downloadAttachments() {
            const log = $log.get("attachmentDataSynchronizationService#downloadAttachments", ["attachment", "sync", "download"]);

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
                bufferedDownload(fullPromiseBuffer, deferred, log);

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



            const ids = dataEntries.map(entry => entry.id).join("','");


            const queryObj = { query: entities.Attachment.ByApplicationAndIds, args: [applicationName, ids] };
            return swdbDAO.executeQuery(queryObj).then(attachments => {
                if (attachments.length === 0) {
                    log.debug("no attachments to uppload for application {0}".format(applicationName));
                    return $q.when(dataEntries);
                }
                attachments.forEach(attachment => {
                    var dataEntry = dataEntries.find(f => f.id === attachment.parentId);
                    var attachmentArray = dataEntry.datamap["attachment_"];
                    if (attachmentArray == null) {
                        log.trace("skipping entry {0} since it hold no attachment".format(dataEntry.id));
                        return;
                    }
                    //                    #offlinehash
                    var rightAttachmentDatamap = attachmentArray.find(a => a["#offlinehash"] === attachment.id);
                    if (rightAttachmentDatamap == null) {
                        log.warn(`could not locate attachment ${attachment.id} in any of the ${applicationName} of ids ${ids}`);
                        return;
                    }
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