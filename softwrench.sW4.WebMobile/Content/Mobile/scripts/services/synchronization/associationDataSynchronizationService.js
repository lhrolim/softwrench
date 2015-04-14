mobileServices.factory('associationDataSynchronizationService', function ($http, swdbDAO,dispatcherService) {
    return {

        syncData: function () {
            var deferred = dispatcherService.loadBaseDeferred();
            return deferred.promise;
        }
    }
 });