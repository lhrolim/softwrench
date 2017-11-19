(function (angular) {
    'use strict';

    var buildIdsString = function (deletedRecordIds) {
        var ids = [];
        angular.forEach(deletedRecordIds, function (id) {
            ids.push("'{0}'".format(id));
        });
        return ids;
    };

    class fsDataSyncHook {

        

        constructor(searchIndexService, offlineEntities,fsLaborOfflineService) {
            this.searchIndexService = searchIndexService;
            this.entities = offlineEntities;
            this.fsLaborOfflineService = fsLaborOfflineService;
        }

        modifyQueries(result, queryArray) {

            //combining all arrays into one (several compositions, one per each type of workorder)
            const labtransEntries = 
                _.chain(result.compositionData)
                .filter(c => c.applicationName === "labtrans_")
                .pluck("newdataMaps")
                .flatten()
                .value();

            if (labtransEntries.length > 0) {
                const application = result.compositionData.find(c => c.applicationName === "labtrans_")
                const syncedwos = new Set();
                labtransEntries.forEach(element => {
                    const toInsertObj = this.fsLaborOfflineService.generateTsLaborDataEntryQuery(element);
                    queryArray.push(toInsertObj.query);
                    syncedwos.add(toInsertObj.wonum)
                });
                const ids = buildIdsString(Array.from(syncedwos));
                queryArray.push({query: "delete from DataEntry where application = 'tslabor' and rowstamp is null and textindex02 in ({0})".format(ids)});
            }
            

            return null;
        }

    }


    fsDataSyncHook['$inject'] = ["searchIndexService", "offlineEntities", "fsLaborOfflineService"];

    angular.module('sw_mobile_services').service('firstsolar.dataSynchronizationHook', fsDataSyncHook);

})(angular);