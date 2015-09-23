(function (mobileServices) {
    "use strict";

    var buildIdsString = function (deletedRecordIds) {
        var ids = [];
        angular.forEach(deletedRecordIds, function (id) {
            ids.push("'{0}'".format(id));
        });
        return ids;
    };

    var service = function ($http, $q, $log, swdbDAO, dispatcherService, restService, metadataModelService, rowstampService, offlineCompositionService, entities) {

        var errorHandlePromise = function (error) {
            if (!error) {
                return $q.when();
            }
            return $q.reject(error);
        };



        function resultHandlePromise(result) {
            var log = $log.get("dataSynchronizationService#createAppSyncPromise");
            var topApplicationData = result.data.topApplicationData;
            var compositionData = result.data.compositionData;
            if (result.data.isEmpty) {
                log.info("no new data returned from the server");
                //interrupting async calls
                return 0;
            }

            log.info("receiving new topLevel data from the server");
            var queryArray = [];
            for (var i = 0; i < topApplicationData.length; i++) {
                //multiple applications can be returned on a limit scenario where it´s the first sync, or on a server update.
                var application = topApplicationData[i];
                var newDataMaps = application.newdataMaps;
                var updatedDataMaps = application.updatedDataMaps;
                var insertUpdateDatamap = application.insertOrUpdateDataMaps;
                var deletedIds = application.deletedRecordIds;
                log.debug("{0} topleveldata: inserting:{1} | updating:{2} | deleting: {3}".format(application.applicationName, newDataMaps.length, updatedDataMaps.length, deletedIds.length));
                for (var j = 0; j < newDataMaps.length; j++) {
                    var newDataMap = newDataMaps[j];
                    var id = persistence.createUUID();
                    var newJson = JSON.stringify(newDataMap.fields);
                    //newJson = datamapSanitizationService.sanitize(newJson);

                    var insertQuery = { query: entities.DataEntry.insertionQueryPattern, args: [newDataMap.application, newJson, newDataMap.id, String(newDataMap.approwstamp), id] };
                    queryArray.push(insertQuery);
                }

                for (j = 0; j < insertUpdateDatamap.length; j++) {
                    var insertOrUpdateDatamap = insertUpdateDatamap[j];
                    var id = persistence.createUUID();
                    var newJson = JSON.stringify(insertOrUpdateDatamap.fields);
                    //newJson = datamapSanitizationService.sanitize(newJson);

                    var insertOrUpdateQuery = { query: entities.DataEntry.insertOrReplacePattern, args: [insertOrUpdateDatamap.application, newJson,newJson, insertOrUpdateDatamap.id, String(insertOrUpdateDatamap.approwstamp), id] };
                    queryArray.push(insertOrUpdateQuery);
                }

                for (j = 0; j < updatedDataMaps.length; j++) {
                    var updateDataMap = updatedDataMaps[j];
                    var updateJson = JSON.stringify(updateDataMap.fields);
                    //updateJson = datamapSanitizationService.sanitize(updateJson);

                    var updateQuery = { query: entities.DataEntry.updateQueryPattern, args: [updateJson, String(updateDataMap.approwstamp), updateDataMap.id, updateDataMap.application] };
                    queryArray.push(updateQuery);
                }

                if (deletedIds.length > 0) {
                    var deleteQuery = { query: entities.DataEntry.deleteQueryPattern, args: [buildIdsString(deletedIds), application.applicationName] };
                    queryArray.push(deleteQuery);
                    //TODO: treat the case where AuditEntries that have no refId shouldn't be deleted (e.g. crud_create operations)
                    var deleteAuditQuery = { query: entities.AuditEntry.deleteRelatedByRefIdStatement, args: [application.apllicationName, buildIdsString(deletedIds)] };
                    queryArray.push(deleteAuditQuery);
                }
            }
            //ignoring composition number to SyncOperation table
            var numberOfDownloadedItems = queryArray.length;
            queryArray = queryArray.concat(offlineCompositionService.generateSyncQueryArrays(compositionData));
            return swdbDAO.executeQueries(queryArray).then(function () {
                return $q.when(numberOfDownloadedItems);
            });
        };


        function createAppSyncPromise(firstInLoop, app, currentApps, compositionMap) {
            var log = $log.get("dataSynchronizationService#createAppSyncPromise");

            var params = {
                applicationName: app,
                clientCurrentTopLevelApps: currentApps,
                returnNewApps: firstInLoop
            }
            return rowstampService.generateRowstampMap(app)
                .then(function (rowstampMap) {
                    //see samplerequest.json
                    rowstampMap.compositionmap = compositionMap;
                    log.debug("invoking service to get new data");
                    return restService.postPromise("Mobile", "PullNewData", params, rowstampMap);
                }).then(resultHandlePromise);
        };

        function syncData() {
            var currentApps = metadataModelService.getApplicationNames();
            var firstTime = currentApps.length === 0;
            var params;
            if (firstTime) {
                //upon first synchronization let's just bring them all, since we don´t even know what are the metadatas
                params = {
                    clientCurrentTopLevelApps: currentApps,
                    returnNewApps: true
                };
                //single server call
                return restService.postPromise("Mobile", "PullNewData", params)
                    .then(resultHandlePromise)
                    .catch(errorHandlePromise);
            }

            return rowstampService.generateCompositionRowstampMap()
                .then(function (compositionMap) {
                    var httpPromises = [];
                    for (var i = 0; i < currentApps.length; i++) {
                        var promise = createAppSyncPromise(i === 0, currentApps[i], currentApps, compositionMap).catch(errorHandlePromise);
                        httpPromises.push(promise);
                    }
                    return $q.all(httpPromises);
                });
        };

        var api = {
            syncData: syncData
        };

        return api;
    };

    service.$inject = ["$http", "$q", "$log", "swdbDAO", "dispatcherService", "restService", "metadataModelService", "rowstampService", "offlineCompositionService", "offlineEntities"];

    mobileServices.factory('dataSynchronizationService', service);

})(mobileServices);
