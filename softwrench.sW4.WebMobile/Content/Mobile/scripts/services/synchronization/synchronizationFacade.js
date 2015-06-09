mobileServices.factory('synchronizationFacade', function ($http, $log, $q, routeService,
    dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService, synchronizationOperationService) {

    return {

        fullSync: function (currentData) {

            var log = $log.get("synchronizationFacade#fullSync");

            log.info("init full synchronization process");
            var start = new Date().getTime();

            var dbapplications = metadataModelService.getMetadatas();

            //one per application
            var batchPromises = [];

            for (var i = 0; i < dbapplications.length; i++) {
                var dbapplication = dbapplications[i];
                batchPromises.push(batchService.createBatch(dbapplication));
            }
            var batchsSubmited = false;
            return $q.all(batchPromises)
                .then(function (batches) {
                    if (batches[0] != null) {
                        log.info('batches created locally, server will respond asynchronously');
                        batchsSubmited = true;
                        return batchService.submitBatches(batches);
                    }
                    var syncDataPromise = dataSynchronizationService.syncData(batches);
                    var metadataDownloadedPromise = metadataSynchronizationService.syncData("1.0");
                    var associationDataDownloadPromise = associationDataSynchronizationService.syncData();
                    var httpPromises = [];
                    httpPromises.push(metadataDownloadedPromise);
                    httpPromises.push(associationDataDownloadPromise);
                    httpPromises = httpPromises.concat(syncDataPromise);
                    return $q.all(httpPromises);
                }).catch(function (err) {
                    return $q.reject(false);
                }).then(function (results) {
                    if (batchsSubmited) {
                        return synchronizationOperationService.createBatchOperation(start, results);
                    }
                    var end = new Date().getTime();
                    log.info("finished full synchronization process. Ellapsed {0}".format(end - start));
                    var metadataDownloadedResult = results[0];
                    var associationDataDownloaded = results[1];
                    var dataDownloadedResult = results.subarray(2);
                    var totalNumber = 0;
                    for (var j = 0; j < dataDownloadedResult.length; j++) {
                        totalNumber += dataDownloadedResult[j];
                    }
                    synchronizationOperationService.createNonBatchOperation(start, end,totalNumber,associationDataDownloaded,metadataDownloadedResult);
                });

        },


    }

});
