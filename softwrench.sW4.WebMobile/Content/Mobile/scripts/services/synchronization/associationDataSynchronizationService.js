mobileServices.factory('associationDataSynchronizationService', function ($http, swdbDAO,dispatcherService) {
    return {

        syncData: function () {
            var deferred = dispatcherService.loadBaseDeferred();
            deferred.resolve();
            return deferred.promise;
        }
    }
 });