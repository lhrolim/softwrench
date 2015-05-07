mobileServices.factory('dataSynchronizationService', function ($http, $q, dispatcherService, restService, metadataModelService) {
    return {

        syncData: function (workbatchs) {
            var deferred = dispatcherService.loadBaseDeferred();

            var currentApps = metadataModelService.getApplicationNames();
            var firstTime = currentApps.length == 0;
            var params;
            if (firstTime) {
                //upon first synchronization let's just bring them all
                params = {
                    clientCurrentTopLevelApps: currentApps,
                    returnNewApps: true
                };
                //single server call
                return restService.postPromise("Mobile", "PullNewData", params);
            } else {
                var httpPromises = [];
                for (var i = 0; i < currentApps.length; i++) {
                    var app = currentApps[i];
                    //TODO: make it
                    var rowstampMap = {};
                    params = {
                        applicationName: app,
                        clientCurrentTopLevelApps: currentApps,
                        returnNewApps: i==0
                    }
                    var promise = restService.postPromise("Mobile", "PullNewData", params, rowstampMap).then(function(result) {
                        var topApplicationData = result.topApplicationData;
                        var compositionData = result.compositionData;
                        return result;
                    });
                    httpPromises.push(promise);
                }
            }




            return httpPromises;
        }
    }
});