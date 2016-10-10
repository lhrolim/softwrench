(function (mobileServices, angular, _) {
    "use strict";

    function synchronizationFacade($log, $q, dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService, synchronizationOperationService,
        asyncSynchronizationService, synchronizationNotificationService, offlineAuditService, dao, loadingService, $ionicPopup, crudConstants, entities, problemService, tracking) {

        //#region Utils

        function getDownloadDataCount(dataDownloadResult) {
            let count = 0;
            angular.forEach(dataDownloadResult, result => {
                if (!angular.isArray(result)) {
                    count += result;
                    return;
                }
                // in some cases, each result is an array of numbers
                // in that case we need to iterate through each number of result
                angular.forEach(result, element => count += element);
            });
            return count;
        }

        /**
         * Deletes the 'deletable' DataEntries related to the batches.
         * A 'deletable' DataEntry is one which's related BatchItem: 
         * - has a crudoperation === crudConstants.operation.create
         * - has no problem associated with it
         * 
         * @param [Batch] batches 
         * @returns Promise resolved with the batches, rejected with database error 
         */
        function handleDeletableDataEntries(batches) {
            const statements = _.chain(batches)
                .pluck("loadeditems") // [[BatchItem]]
                .flatten() // [BatchItem]
                .filter(item => item.crudoperation === crudConstants.operation.create && !item.problem) // [BatchItem] crud_create and doesn't have problem  
                .pluck("dataentry") // [DataEntry]
                .groupBy("application") // { DataEntry['application'] : [DataEntry] }
                .map((entries, application) => {
                    // for some reason this query only works if there are no '' around the ids
                    var ids = _.pluck(entries, "id");
                    return { query: entities.DataEntry.deleteInIdsStatement, args: [ids, application] };
                }) // [PreparedStatement]
                .value();

            // nothing to delete: resolve with batches immediately
            if (statements.length <= 0) return $q.when(batches);
            // resolve with batches if delete wass successful
            return dao.executeQueries(statements).then(() => batches);
        }

        /**
         * Updates the Batches, deletes DataEntries that should be deleted 
         * and creates a SyncOperation.
         * 
         * @param Object completionResult 
         */
        function onBatchesCompleted(completionResult) {
            const log = $log.get("dataSynchronizationService#onBatchesCompleted");
            const start = completionResult.start;
            const batchTuples = completionResult.batchTuples;
            const promises = _.map(batchTuples, tuple => {
                const remoteBatch = tuple.remote;
                const batch = tuple.local;
                return batchService.updateBatch(batch, remoteBatch);
            });
            $q.all(promises)
                .then(batches => handleDeletableDataEntries(batches))
                .then(batches => problemService.updateHasProblemToDataEntries(batches))
                // update the related syncoperations as 'COMPLETE'
                // TODO: assuming there's only a single batch/application per syncoperation -> develop generic case
                .then(batches => synchronizationOperationService.completeFromAsyncBatch(batches)
                // resolve with the saved batches to transparently continue the promise 
                // chain as it was before (not aware of syncoperations update)
                .then(operations => batches))
                .then(batches =>
                    dataSynchronizationService.syncData().then(downloadResults => {
                        var dataCount = getDownloadDataCount(downloadResults);
                        return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batches);
                    })
                )
                .then(operation => {
                    log.info("created SyncOperation for async Batch Processing");
                    synchronizationNotificationService.notifySynchronizationReceived(operation);
                })
                .catch(error => log.error(error));
        }

        /**
         * For each item in the payload adds the related AuditEntries in their 'additionaldata' field.
         * 
         * @returns Promise resolved with the updated payload
         */
        function onBeforeBatchSubmit(batch, params, payload) {
            const promises = payload.items.map(item =>
                offlineAuditService.getEntriesForEntity(item.dataentry, batch.application).then(entries => {
                    item.additionaldata.auditentries = entries;
                    return item;
                }));
            return $q.all(promises).then(items => {
                // substitute payload's items by items with auditentries
                payload.items = items;
                return payload;
            });
        }

        //#endregion

        //#region Public methods

        function hasDataToSync() {
            return dao.countByQuery("DataEntry", "isDirty=1 and pending=0").then(count => count > 0);
        }

        /**
         * Executes a full download (data, metadata and association data) and creates a SyncOperation
         * reflecting the execution.
         * 
         * @returns Promise: resolved with created SyncOperation; rejected with HTTP or Database error 
         */
        function fullDownload() {
            const log = $log.get("synchronizationFacade#fullDownload");
            log.info("Executing full download");
            const start = new Date().getTime();

            const httpPromises = [
                metadataSynchronizationService.syncData("1.0"),
                associationDataSynchronizationService.syncData()
            ].concat(dataSynchronizationService.syncData());

            return $q.all(httpPromises)
                .then(results => {
                    const end = new Date().getTime();
                    log.info("finished full download process. Ellapsed {0}".format(end - start));

                    const metadataDownloadedResult = results[0];
                    const associationDataDownloaded = results[1];
                    const dataDownloadedResult = results.subarray(2);
                    const totalNumber = getDownloadDataCount(dataDownloadedResult);

                    return synchronizationOperationService.createNonBatchOperation(start, end, totalNumber, associationDataDownloaded, metadataDownloadedResult);
                });
        }

        /**
         * If the user caused a Batches to be created (altering and/or creating new content) then the Batches 
         * will be submitted to the server: If it receives a synchronous response from the server 
         * a data download will be executed.
         * Otherwise (no Batches) a full download (data, metadata and association data) will be executed. 
         * In any case a SyncOperation reflecting the method execution (Batch or Batach+download or full download) will be created.
         * 
         * @returns Promise: resolved with created SyncOperation; rejected with HTTP or Database error
         */
        function fullSync() {
            const log = $log.get("synchronizationFacade#fullSync", ["sync"]);
            log.info("init full synchronization process");
            tracking.trackFullState("synchornizationFacace#fullSync pre-sync");

            const start = new Date().getTime();
            const dbapplications = metadataModelService.getMetadatas();

            // one Batch per application
            const batchPromises = dbapplications.map(dbapplication => batchService.createBatch(dbapplication));

            return $q.all(batchPromises)
                .then(batches => {
                    // no batches created: full download instead of full sync
                    if (!batches || batches.length <= 0 || !batches.some(s=> s != null)) {
                        log.info("No batches created: Executing full download instead of full sync.");
                        return fullDownload();
                    }
                    // batches created: submit to server
                    log.info("Batches created locally: submitting to server.");
                    return batchService.submitBatches(batches).then(batchResults => {
                        // check for synchronous or asynchronous case
                        var asyncBatches = batchResults.filter(batch => batch.status !== "COMPLETE");
                        // async case
                        if (asyncBatches.length > 0) {
                            // register async Batches for async processing
                            angular.forEach(asyncBatches, asyncBatch => asyncSynchronizationService.registerForAsyncProcessing(asyncBatch));
                            // create batch/offline SyncOperation
                            return synchronizationOperationService.createBatchOperation(start, batchResults);
                        }
                        // sync case: 
                        // - delete DataEntries that should be deleted
                        // - updates the hasProblem flag on DataEntries
                        // - download ONLY data and create a SyncOperation indicating both a Batch submission and a download
                        return handleDeletableDataEntries(batchResults)
                            .then(() => problemService.updateHasProblemToDataEntries(batchResults))
                            .then(() => dataSynchronizationService.syncData())
                            .then(downloadResults => {
                                log.debug("Batch returned synchronously --> performing download");
                                var dataCount = getDownloadDataCount(downloadResults);
                                return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batchResults);
                            });
                    });
                })
                .finally(() => {
                    tracking.trackFullState("synchornizationFacace#fullSync post-sync");
                });
        }

        function syncItem(item) {
            const log = $log.get("synchronizationFacade#syncItem", ["sync"]);
            log.info("init quick sync process");
            tracking.trackFullState("synchornizationFacace#syncItem pre-quicksync");

            const dbapplication = metadataModelService.getMetadatas().find(a => a.application === item.application);
            const start = new Date().getTime();
            loadingService.showDefault();
            // one Batch per application
            return batchService.createBatch(dbapplication, item)
                    .then(batch => batchService.submitBatches([batch]))
                    .then(batchResults => {
                        return handleDeletableDataEntries(batchResults)
                            .then(() => problemService.updateHasProblemToDataEntries(batchResults, item))
                            .then(() => dataSynchronizationService.syncSingleItem(item))
                            .then(downloadResults => {
                                var dataCount = getDownloadDataCount(downloadResults);
                                return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batchResults);
                            });
                    })
                   .finally(() => {
                       loadingService.hide();
                       tracking.trackFullState("synchornizationFacace#syncItem post-quicksync");
                    });
        }

        /**
         * Attemps a synchronization.
         * If it succeeds resolve with <code>true</code> indicating sync was successfull.
         * If it fails prompts the user with a confirm popup ("continue anyway?").
         * Resolve with the user's response.
         * Before attempting synchronization show loading; after synchronization (success or fail) loading is toggled off.
         * 
         * @param {} failPopupConfig configuration of the confirm popup. Defaults to {title:"Synchronization failed",template:"Continue anyway?"}
         * @returns Promise resolved with Boolean indicating the caller it can continue it's workflow. 
         */
        function attempSyncAndContinue(failPopupConfig) {
            loadingService.showDefault();

            // try to sync
            return fullSync()
                .then(() => {
                    loadingService.hide();
                    return true;
                })
                .catch(() => {
                    loadingService.hide();
                    // sync failed: check if user wishes to logout regardless
                    return $ionicPopup.confirm({
                        title: failPopupConfig.title || "Synchronization Failed",
                        template: failPopupConfig.template || "Continue Anyway?"
                    })
                    .then(continueAnyway => !!continueAnyway);
                });
        }

        //#endregion

        //#region Service instance

        // registering batch submit callback on batchService
        batchService.onBeforeBatchSubmit(onBeforeBatchSubmit);
        // registering completion callback on the asyncSynchronizationService
        asyncSynchronizationService.onBatchesCompleted(onBatchesCompleted);

        const api = {
            hasDataToSync,
            fullDownload,
            fullSync,
            syncItem,
            attempSyncAndContinue,
            handleDeletableDataEntries
        };
        return api;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("synchronizationFacade", ["$log", "$q", "dataSynchronizationService", "metadataSynchronizationService", "associationDataSynchronizationService", "batchService",
        "metadataModelService", "synchronizationOperationService", "asyncSynchronizationService", "synchronizationNotificationService", "offlineAuditService", "swdbDAO", "loadingService", "$ionicPopup", "crudConstants", "offlineEntities", "problemService", "trackingService", synchronizationFacade]);
    //#endregion

})(mobileServices, angular, _);