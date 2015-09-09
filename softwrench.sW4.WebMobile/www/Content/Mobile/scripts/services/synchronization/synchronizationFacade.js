﻿(function (mobileServices, angular) {
    "use strict";

    function synchronizationFacade($log, $q, dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService, synchronizationOperationService, asyncSynchronizationService, synchronizationNotificationService, offlineAuditService, swdbDAO, $ionicLoading, $ionicPopup) {
        
        //#region Utils

        function getDownloadDataCount(dataDownloadResult) {
            var count = 0;
            angular.forEach(dataDownloadResult, function (result) {
                if (!angular.isArray(result)) {
                    count += result;
                    return;
                }
                // in some cases, each result is an array of numbers
                // in that case we need to iterate through each number of result
                angular.forEach(result, function (element) {
                    count += element;
                });
            });
            return count;
        }

        /**
         * Updates the Batches and creates a SyncOperation.
         * 
         * @param Object completionResult 
         */
        function onBatchesCompleted(completionResult) {
            var log = $log.get("dataSynchronizationService#onBatchesCompleted");
            var start = completionResult.start;
            var batchTuples = completionResult.batchTuples;
            var promises = [];
            angular.forEach(batchTuples, function (tuple) {
                var remoteBatch = tuple.remote;
                var batch = tuple.local;
                promises.push(batchService.updateBatch(batch, remoteBatch));
            });
            $q.all(promises)
                .then(function (batches) {
                    // update the related syncoperations as 'COMPLETE'
                    // TODO: assuming there's only a single batch/application per syncoperation -> develop generic case
                    return synchronizationOperationService.completeFromAsyncBatch(batches).then(function (operations) {
                        // resolve with the saved batches to transparently continue the promise 
                        // chain as it was before (not aware of syncoperations update)
                        return batches;
                    });
                })
                .then(function (batches) {
                    return dataSynchronizationService.syncData().then(function (downloadResults) {
                        var dataCount = getDownloadDataCount(downloadResults);
                        return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batches);
                    });
                })
                .then(function (operation) {
                    log.info("created SyncOperation for async Batch Processing");
                    synchronizationNotificationService.notifySynchronizationReceived(operation);
                })
                .catch(function (error) {
                    log.error(error);
                });
        }

        /**
         * For each item in the payload adds the related AuditEntries in their 'additionaldata' field.
         * 
         * @returns Promise resolved with the updated payload
         */
        function onBeforeBatchSubmit(batch, params, payload) {
            var promises = [];
            angular.forEach(payload.items, function (item) {
                var promise = offlineAuditService.getEntriesForEntity(item.dataentry, batch.application).then(function (entries) {
                    item.additionaldata.auditentries = entries;
                    return item;
                });
                promises.push(promise);
            });
            return $q.all(promises).then(function (items) {
                // substitute payload's items by items with auditentries
                payload.items = items;
                return payload;
            });
        }

        //#endregion

        //#region Public methods

        function hasDataToSync() {
            return swdbDAO.countByQuery("DataEntry", "isDirty=1 and pending=0").then(function(count) {
                return count > 0;
            });
        }

        /**
         * Executes a full download (data, metadata and association data) and creates a SyncOperation
         * reflecting the execution.
         * 
         * @returns Promise: resolved with created SyncOperation; rejected with HTTP or Database error 
         */
        function fullDownload() {
            var log = $log.get("synchronizationFacade#fullDownload");
            log.info("Executing full download");
            var start = new Date().getTime();
            var syncDataPromise = dataSynchronizationService.syncData();
            var metadataDownloadedPromise = metadataSynchronizationService.syncData("1.0");
            var associationDataDownloadPromise = associationDataSynchronizationService.syncData();
            var httpPromises = [];
            httpPromises.push(metadataDownloadedPromise);
            httpPromises.push(associationDataDownloadPromise);
            httpPromises = httpPromises.concat(syncDataPromise);
            return $q.all(httpPromises)
                .then(function(results) {
                    var end = new Date().getTime();
                    log.info("finished full download process. Ellapsed {0}".format(end - start));
                    var metadataDownloadedResult = results[0];
                    var associationDataDownloaded = results[1];
                    var dataDownloadedResult = results.subarray(2);
                    var totalNumber = getDownloadDataCount(dataDownloadedResult);
                    
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
            var log = $log.get("synchronizationFacade#fullSync");
            log.info("init full synchronization process");

            var start = new Date().getTime();
            var dbapplications = metadataModelService.getMetadatas();

            // one Batch per application
            var batchPromises = dbapplications.map(function(dbapplication) {
                return batchService.createBatch(dbapplication);
            });
           
            return $q.all(batchPromises)
                .then(function (batches) {
                    // no batches created: full download instead of full sync
                    if (!batches || batches.length <= 0 || !batches[0]) {
                        log.info("No batches created: Executing full download instead of full sync.");
                        return fullDownload();
                    }
                    // batches created: submit to server
                    log.info("Batches created locally: submitting to server.");
                    return batchService.submitBatches(batches)
                        .then(function (batchResults) {
                            // check for synchronous or asynchronous case
                            var asyncBatches = batchResults.filter(function(batch) {
                                return batch.status !== "COMPLETE";
                            });
                            // async case
                            if (asyncBatches.length > 0) {
                                // register async Batches for async processing
                                angular.forEach(asyncBatches, function (asyncBatch) {
                                    asyncSynchronizationService.registerForAsyncProcessing(asyncBatch);
                                });
                                // create batch/offline SyncOperation
                                return synchronizationOperationService.createBatchOperation(start, batchResults);
                            }
                            // sync case: download ONLY data and create a SyncOperation indicating both a Batch submission and a download
                            return dataSynchronizationService.syncData()
                                .then(function (downloadResults) {
                                    log.debug("Batch returned synchronously --> performing download");
                                    var dataCount = getDownloadDataCount(downloadResults);
                                    return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batchResults);
                                });
                        });
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
            $ionicLoading.show({
                template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Synchronizing data<span>"
            });
            // try to sync
            return fullSync().then(function () {
                    $ionicLoading.hide();
                    return true;
                }).catch(function () {
                    $ionicLoading.hide();
                    // sync failed: check if user wishes to logout regardless
                    return $ionicPopup.confirm({
                        title: failPopupConfig.title || "Synchronization failed",
                        template: failPopupConfig.template || "Continue anyway?"
                    }).then(function (continueAnyway) {
                        return !!continueAnyway;
                    });
                });
        }

        //#endregion

        //#region Service instance

        // registering batch submit callback on batchService
        batchService.onBeforeBatchSubmit(onBeforeBatchSubmit);
        // registering completion callback on the asyncSynchronizationService
        asyncSynchronizationService.onBatchesCompleted(onBatchesCompleted);

        var api = {
            hasDataToSync: hasDataToSync,
            fullDownload: fullDownload,
            fullSync: fullSync,
            attempSyncAndContinue: attempSyncAndContinue
        }

        return api;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("synchronizationFacade", ["$log", "$q", "dataSynchronizationService", "metadataSynchronizationService", "associationDataSynchronizationService", "batchService", "metadataModelService", "synchronizationOperationService", "asyncSynchronizationService", "synchronizationNotificationService", "offlineAuditService", "swdbDAO", "$ionicLoading", "$ionicPopup", synchronizationFacade]);
    //#endregion

})(mobileServices, angular);