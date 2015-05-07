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
                    httpPromises.push(restService.postPromise("Mobile", "PullNewData", params, rowstampMap));
                }
            }




            return httpPromises;
        }
    }
});