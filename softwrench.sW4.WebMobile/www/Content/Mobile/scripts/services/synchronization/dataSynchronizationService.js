(function (mobileServices, angular) {
    "use strict";

    var buildIdsString = function (deletedRecordIds) {
        var ids = [];
        angular.forEach(deletedRecordIds, function (id) {
            ids.push("'{0}'".format(id));
        });
        return ids;
    };
    const service = function ($http, $q, $log, swdbDAO, dispatcherService, restService, metadataModelService, rowstampService, offlineCompositionService, entities) {

        var errorHandlePromise = function (error) {
            if (!error) {
                return $q.when();
            }
            return $q.reject(error);
        };



        function resultHandlePromise(result) {
            const log = $log.get("dataSynchronizationService#createAppSyncPromise");
            const topApplicationData = result.data.topApplicationData;
            const compositionData = result.data.compositionData;
            if (result.data.isEmpty) {
                log.info("no new data returned from the server");
                //interrupting async calls
                return 0;
            }

            log.info("receiving new topLevel data from the server");
            var queryArray = [];
            for (let i = 0; i < topApplicationData.length; i++) {
                //multiple applications can be returned on a limit scenario where it´s the first sync, or on a server update.
                const application = topApplicationData[i];
                const newDataMaps = application.newdataMaps;
                const updatedDataMaps = application.updatedDataMaps;
                const insertUpdateDatamap = application.insertOrUpdateDataMaps;
                const deletedIds = application.deletedRecordIds;
                log.debug("{0} topleveldata: inserting:{1} | updating:{2} | deleting: {3}".format(application.applicationName, newDataMaps.length, updatedDataMaps.length, deletedIds.length));

                angular.forEach(newDataMaps, function (newDataMap) {
                    const id = persistence.createUUID();
                    const newJson = JSON.stringify(newDataMap.fields); //newJson = datamapSanitizationService.sanitize(newJson);
                    const insertQuery = { query: entities.DataEntry.insertionQueryPattern, args: [newDataMap.application, newJson, newDataMap.id, String(newDataMap.approwstamp), id] };
                    queryArray.push(insertQuery);
                });

                angular.forEach(insertUpdateDatamap, function (insertOrUpdateDatamap) {
                    const id = persistence.createUUID();
                    const newJson = JSON.stringify(insertOrUpdateDatamap.fields); //newJson = datamapSanitizationService.sanitize(newJson);
                    const insertOrUpdateQuery = { query: entities.DataEntry.insertOrReplacePattern, args: [insertOrUpdateDatamap.application, newJson, newJson, insertOrUpdateDatamap.id, String(insertOrUpdateDatamap.approwstamp), id] };
                    queryArray.push(insertOrUpdateQuery);
                });

                angular.forEach(updatedDataMaps, function (updateDataMap) {
                    const updateJson = JSON.stringify(updateDataMap.fields); //updateJson = datamapSanitizationService.sanitize(updateJson);
                    const updateQuery = { query: entities.DataEntry.updateQueryPattern, args: [updateJson, String(updateDataMap.approwstamp), updateDataMap.id, updateDataMap.application] };
                    queryArray.push(updateQuery);
                });

                if (deletedIds.length > 0) {
                    const deleteQuery = { query: entities.DataEntry.deleteQueryPattern, args: [buildIdsString(deletedIds), application.applicationName] };
                    queryArray.push(deleteQuery);
                    //TODO: treat the case where AuditEntries that have no refId shouldn't be deleted (e.g. crud_create operations)
                    const deleteAuditQuery = { query: entities.AuditEntry.deleteRelatedByRefIdStatement, args: [application.apllicationName, buildIdsString(deletedIds)] };
                    queryArray.push(deleteAuditQuery);
                }
            }
            //ignoring composition number to SyncOperation table
            const numberOfDownloadedItems = queryArray.length;
            queryArray = queryArray.concat(offlineCompositionService.generateSyncQueryArrays(compositionData));
            return swdbDAO.executeQueries(queryArray).then(() =>  $q.when(numberOfDownloadedItems));
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
                    return restService.post("Mobile", "PullNewData", params, rowstampMap);
                }).then(resultHandlePromise);
        };

        function syncData() {
            var currentApps = metadataModelService.getApplicationNames();
            const firstTime = currentApps.length === 0;
            var params;
            if (firstTime) {
                //upon first synchronization let's just bring them all, since we don´t even know what are the metadatas
                params = {
                    clientCurrentTopLevelApps: currentApps,
                    returnNewApps: true
                };
                //single server call
                return restService.post("Mobile", "PullNewData", params)
                    .then(resultHandlePromise)
                    .catch(errorHandlePromise);
            }

            return rowstampService.generateCompositionRowstampMap()
                .then(function (compositionMap) {
                    const httpPromises = [];
                    for (let i = 0; i < currentApps.length; i++) {
                        const promise = createAppSyncPromise(i === 0, currentApps[i], currentApps, compositionMap).catch(errorHandlePromise);
                        httpPromises.push(promise);
                    }
                    return $q.all(httpPromises);
                });
        };

        const api = {
            syncData
        };

        return api;
    };
    service.$inject = ["$http", "$q", "$log", "swdbDAO", "dispatcherService", "offlineRestService", "metadataModelService", "rowstampService", "offlineCompositionService", "offlineEntities"];

    mobileServices.factory('dataSynchronizationService', service);

})(mobileServices, angular);
