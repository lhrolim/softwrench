mobileServices.factory('dataSynchronizationService', function ($http,dispatcherService) {
    return {

        syncData: function () {
            var deferred = dispatcherService.loadBaseDeferred();
            return deferred.promise;
        }
    }
 });