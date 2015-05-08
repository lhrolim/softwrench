mobileServices.factory('dataSynchronizationService', function ($http, $q,$log, swdbDAO, dispatcherService, restService, metadataModelService, rowstampService) {

    function createAppSyncPromise(firstInLoop,app, currentApps) {
        var log = $log.get("dataSynchronizationService#createAppSyncPromise");

        var params = {
            applicationName: app,
            clientCurrentTopLevelApps: currentApps,
            returnNewApps: firstInLoop
        }
        return rowstampService.generateRowstampMap(app).then(function (rowstampMap) {
            log.debug("invoking service to get new data");
            return restService.postPromise("Mobile", "PullNewData", params, rowstampMap);
        }).then(function (result) {
            log.info("receiving new topLevel data from the server");
            var topApplicationData = result.topApplicationData;
            var compositionData = result.compositionData;
            if (topApplicationData.length == 0 && compositionData.length == 0) {
                //interrupting async calls
                return $q.reject();
            }
            return swdbDAO.createTx(result);
        }).then(function (txArray) {
            var tx = txArray[0];
            var serverResult = txArray[1];
            var topApplicationData = serverResult.topApplicationData;
            var compositionData = serverResult.compositionData;

            for (var i = 0; i < topApplicationData.dataMaps; i++) {

            }
        }).catch(function (err) {
            if (!err) {
                //normal interruption
                return $q.when();
            }
            return $q.reject(err);
        });
    }



    return {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workbatchs"></param>
        /// <returns type="promise"></returns>
        syncData: function (workbatchs) {


            var currentApps = metadataModelService.getApplicationNames();
            var firstTime = currentApps.length == 0;
            var params;
            if (firstTime) {
                //upon first synchronization let's just bring them all, since we don´t even know what are the metadatas
                params = {
                    clientCurrentTopLevelApps: currentApps,
                    returnNewApps: true
                };
                //single server call
                return restService.postPromise("Mobile", "PullNewData", params);
            }

            var httpPromises = [];
            for (var i = 0; i < currentApps.length; i++) {
                var promise = createAppSyncPromise(i==0,currentApps[i], currentApps);
                httpPromises.push(promise);
            }

            return httpPromises;
        }
    }
});