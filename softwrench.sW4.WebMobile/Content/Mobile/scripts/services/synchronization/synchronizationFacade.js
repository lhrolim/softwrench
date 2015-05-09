mobileServices.factory('synchronizationFacade', function ($http, $log, $q, routeService,
    dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService) {

    return {

        fullSync: function (currentData) {

            var log = $log.get("synchronizationFacade#fullSync");

            log.info("init full synchronization process");

            var applications = metadataModelService.getApplicationNames();

            //one per application
            var batchPromises = [];

            for (var i = 0; i < applications.length; i++) {
                var application = applications[i];
                batchPromises.push(batchService.createBatch(application));
            }

            return $q.all(batchPromises)
                .then(function (batches) {
                    log.info('batches created locally');
                    var batchSubmissionPromise = batchService.submitBatches(batches);
                    var syncDataPromise = dataSynchronizationService.syncData(batches);
                    var metadataDownloadedPromise = metadataSynchronizationService.syncData("1.0");
                    var associationDataDownloadPromise = associationDataSynchronizationService.syncData();
                    var httpPromises = [];
                    httpPromises.push(batchSubmissionPromise);
                    httpPromises.push(metadataDownloadedPromise);
                    httpPromises.push(associationDataDownloadPromise);
                    httpPromises=httpPromises.concat(syncDataPromise);

                    return $q.all(httpPromises);
                }).catch(function (err) {
                    return $q.reject(false);
                }).then(function (results) {
                    log.info("finished full synchronization process");
                });

        },


    }

});
