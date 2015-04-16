mobileServices.factory('synchronizationFacade', function ($http, $log, $q, routeService, dataSynchronizationService, metadataSynchronizationService, associationDataSynchronizationService) {

    return {

        fullSync: function (currentData) {

            var syncDataPromise = dataSynchronizationService.syncData();
            
            var metadataDownloadedPromise = metadataSynchronizationService.syncData("1.0");

            var associationDataDownloadPromise = associationDataSynchronizationService.syncData();

            return $q.all([syncDataPromise, metadataDownloadedPromise, associationDataDownloadPromise]).then(function (results) {
                var dataSyncResult = results[0];
                if (dataSyncResult.error) {
                    return false;
                }
                return true;
            });

        },
      

    }

});