var buildIdsString = function (deletedRecordIds) {
    var ids = "";
    for (var i = 0; i < deletedRecordIds.length; i++) {
        ids += "'" + deletedRecordIds[i] + "'";
        if (i != deletedRecordIds.length - 1) {
            ids += ",";
        }
    }
    return ids;
};

mobileServices.factory('dataSynchronizationService', function ($http, $q, $log, swdbDAO, dispatcherService, restService, metadataModelService, rowstampService, offlineCompositionService) {

    function createAppSyncPromise(firstInLoop, app, currentApps, compositionMap) {
        var log = $log.get("dataSynchronizationService#createAppSyncPromise");

        var params = {
            applicationName: app,
            clientCurrentTopLevelApps: currentApps,
            returnNewApps: firstInLoop
        }
        return rowstampService.generateRowstampMap(app).then(function (rowstampMap) {
            //see samplerequest.json
            rowstampMap.compositionmap = compositionMap;
            log.debug("invoking service to get new data");
            return restService.postPromise("Mobile", "PullNewData", params, rowstampMap);
        }).then(function (result) {

            var topApplicationData = result.data.topApplicationData;
            var compositionData = result.data.compositionData;
            if (result.data.isEmpty) {
                log.info("no new data returned from the server");
                //interrupting async calls
                return $q.reject();
            }

            log.info("receiving new topLevel data from the server");
            var queryArray = [];
            for (var i = 0; i < topApplicationData.length; i++) {
                //multiple applications can be returned on a limit scenario where it´s the first sync, or on a server update.
                var application = topApplicationData[i];
                var newDataMaps = application.newdataMaps;
                var updatedDataMaps = application.updatedDataMaps;
                var deletedIds = application.deletedRecordIds;
                log.debug("{0} topleveldata: inserting:{1} | updating:{2} | deleting: {3}".format(application.applicationName, newDataMaps.length, updatedDataMaps.length, deletedIds.length));
                for (var j = 0; j < newDataMaps.length; j++) {

                    var datamap = newDataMaps[j];
                    var id = persistence.createUUID();
                    var query = entities.DataEntry.insertionQueryPattern.format(datamap.application, JSON.stringify(datamap.fields), datamap.id, '' + datamap.approwstamp, id);
                    queryArray.push(query);
                }
                for (j = 0; j < updatedDataMaps.length; j++) {
                    datamap = updatedDataMaps[j];
                    query = entities.DataEntry.updateQueryPattern.format(JSON.stringify(datamap.fields), '' + datamap.approwstamp, datamap.id, datamap.application);
                    queryArray.push(query);
                }

                if (deletedIds.length > 0) {
                    query = entities.DataEntry.deleteQueryPattern.format(buildIdsString(deletedIds), application.applicationName);
                    queryArray.push(query);
                }
            }
            queryArray = queryArray.concat(offlineCompositionService.generateSyncQueryArrays(compositionData));
            return swdbDAO.executeQueries(queryArray);
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

            return rowstampService.generateCompositionRowstampMap()
                .then(function (compositionMap) {
                    var httpPromises = [];
                    for (var i = 0; i < currentApps.length; i++) {
                        var promise = createAppSyncPromise(i == 0, currentApps[i], currentApps, compositionMap);
                        httpPromises.push(promise);
                    }
                    return $q.all(httpPromises);
                });


        }
    }
});