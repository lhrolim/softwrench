(function() {
    "use strict";

    service.$inject = ["$log", "$q", "dataSynchronizationService", "metadataSynchronizationService", "associationDataSynchronizationService", "batchService", "metadataModelService", "synchronizationOperationService"];

    mobileServices.factory('synchronizationFacade', service);

    function service($log, $q, dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService, synchronizationOperationService) {
        var api = {
            fullDownload: fullDownload,
            fullSync: fullSync
        }

        return api;

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
                    
                    synchronizationOperationService.createNonBatchOperation(start, end, totalNumber, associationDataDownloaded, metadataDownloadedResult);
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
                            var hasSyncResponse = batchResults.some(function(result) {
                                return result.status === "COMPLETE";
                            });
                            // async case: create batch/offline SyncOperation
                            if (!hasSyncResponse) {
                                return synchronizationOperationService.createBatchOperation(start, batchResults);
                            }
                            // sync case: download ONLY data and create a SyncOperation indicating both a Batch submission and a download
                            return associationDataSynchronizationService.syncData()
                                .then(function(downloadResults) {
                                    var dataCount = getDownloadDataCount(downloadResults);
                                    return synchronizationOperationService.createSynchronousBatchOperation(start, dataCount, batchResults);
                                });
                        });
                });
        }
    }

})();