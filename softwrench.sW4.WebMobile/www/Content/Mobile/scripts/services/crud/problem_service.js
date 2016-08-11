!(function (angular) {
    "use strict";

    function problemService($q, swdbDAO, offlineEntities) {

        function updateProblem(hasProblem, dataEntriesIds) {
            if (dataEntriesIds.length === 0) {
                return $q.when(null);
            }

            var whereClause = dataEntriesIds.map(id => `id='${id}' or `).join(""); // using id in (?) did not work for some reason...
            whereClause = whereClause.substring(0, whereClause.lastIndexOf(" or "));
            const queryStr = `update DataEntry set hasProblem=${hasProblem ? 1 : 0} where (${whereClause})`;

            return swdbDAO.executeQuery(queryStr);
        }

        function rejectProblematicQuickSync(quickSyncItem) {
            if (quickSyncItem && quickSyncItem.hasProblem) {
                // forces the promise chain to stop and avoid 
                // overriding a problem item with its previous state in case of an update
                return $q.reject("It was not possible to sync.");
            }
            return $q.when(null);
        }

        function locateBatchItemProblems(problemContext, batchItem) {
            const targetIds = batchItem.problem ? problemContext.problematicIds : problemContext.okIds;
            const id = batchItem.dataentry.id;
            if (problemContext.quickSyncItem && problemContext.quickSyncItem.id === id) {
                problemContext.quickSyncItem.hasProblem = !!batchItem.problem;
            }
            targetIds.push(id);
        }

        function locateBatchProblems(problemContext, batch) {
            const deferred = $q.defer();
            persistence.transaction(tx => {
                batch.items.list(tx, batchItems => {
                    angular.forEach(batchItems, batchItem => {
                        locateBatchItemProblems(problemContext, batchItem);
                    });
                    deferred.resolve();
                });
            });
            return deferred.promise;
        }

        function updateHasProblemToDataEntries(batches, quickSyncItem) {
            if (!batches || batches.length === 0) {
                return $q.when(batches);
            }

            const problemContext = {
                okIds: [],
                problematicIds: [],
                quickSyncItem: quickSyncItem
            }

            const batchPromises = [];

            angular.forEach(batches, batch => {
                const batchDeferred = $q.defer();
                batchPromises.push(batchDeferred.promise);

                try {
                    locateBatchProblems(problemContext, batch).then(() => batchDeferred.resolve());
                } catch (e) {
                    batchDeferred.reject(e);
                }
            });

            return $q.all(batchPromises)
                .then(() =>
                    updateProblem(false, problemContext.okIds))
                .then(() =>
                    updateProblem(true, problemContext.problematicIds))
                .then(() =>
                    rejectProblematicQuickSync(quickSyncItem))
                .then(() => batches);
        }

        function getProblems(dataEntryId) {
            return swdbDAO.executeQuery({ query: offlineEntities.DataEntry.findProblems, args: [dataEntryId] });
        }

        const service = {
            updateHasProblemToDataEntries: updateHasProblemToDataEntries,
            getProblems: getProblems
        };

        return service;
    }

    mobileServices.factory("problemService", ["$q", "swdbDAO", "offlineEntities", problemService]);
})(angular);