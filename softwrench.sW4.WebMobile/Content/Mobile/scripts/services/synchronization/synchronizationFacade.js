mobileServices.factory('synchronizationFacade', function ($http, $log, $q, routeService,
    dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService, batchService, metadataModelService) {

    return {

        fullSync: function (currentData) {

            var applications = metadataModelService.getApplicationNames();

            //one per application
            var batchPromises = [];

            for (var i = 0; i < applications.length; i++) {
                var application = applications[i];
                batchPromises.push(batchService.createBatch(application));
            }

            return $q.all(batchPromises)
                .then(function (batches) {
                    var batchSubmissionPromise = batchService.submitBatches(batches);
                    var syncDataPromise = dataSynchronizationService.syncData(batches);
                    var metadataDownloadedPromise = metadataSynchronizationService.syncData("1.0");
                    var associationDataDownloadPromise = associationDataSynchronizationService.syncData();
                    return $q.all([batchSubmissionPromise, syncDataPromise, metadataDownloadedPromise, associationDataDownloadPromise]);
                }).catch(function (err) {
                    return $q.reject(false);
                }).then(function (results) {
                    var dataSyncResult = results[0];
                    if (dataSyncResult.error) {
                        return false;
                    }
                    return true;
                });

        },


    }

});