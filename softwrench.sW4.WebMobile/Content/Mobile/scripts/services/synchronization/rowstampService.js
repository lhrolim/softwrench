mobileServices.factory('rowstampService', function ($q, $log, swdbDAO, offlineEntities) {
    var entities = offlineEntities;

    return {

        generateRowstampMap: function (application) {
            var log = $log.get("rowstampService#generateRowstampMap");
            var deferred = $q.defer();
            var start = new Date().getTime();
            swdbDAO.findByQuery("DataEntry", "application ='{0}'".format(application), { projectionFields: ["remoteId", "rowstamp"] })
                .then(function (queryResults) {
                    var resultItems = [];
                    for (var i = 0; i < queryResults.length; i++) {
                        var item = queryResults[i];
                        var rowstampItem = {
                            id: item.remoteId,
                            rowstamp: item.rowstamp
                        }
                        resultItems.push(rowstampItem);
                    }
                    var rowstampMap = {
                        items: resultItems
                    }
                    var end = new Date().getTime();
                    log.debug("generated rowstampmap for application {0} with {1} entries. Ellapsed {2} ms".format(application, resultItems.length, (end - start)));
                    deferred.resolve(rowstampMap);
                });
            return deferred.promise;
        },

        generateCompositionRowstampMap: function () {
            var log = $log.get("rowstampService#generateCompositionRowstampMap");
            var deferred = $q.defer();
            var start = new Date().getTime();
            swdbDAO.findByQuery('CompositionDataEntry', null, { fullquery: entities.CompositionDataEntry.maxRowstampQueries })
                .then(function (queryResults) {
                    var resultItems = {};
                    for (var i = 0; i < queryResults.length; i++) {
                        var item = queryResults[i];
                        resultItems[item.application] = item.rowstamp;
                    }
                    var end = new Date().getTime();
                    log.debug("generated rowstampmap for compositions. Ellapsed {0} ms".format(end - start));
                    deferred.resolve(resultItems);
                });
            return deferred.promise;
        },

        generateAssociationRowstampMap: function (app) {
            var log = $log.get("rowstampService#generateAssociationRowstampMap");
            var deferred = $q.defer();
            var start = new Date().getTime();
            //either for query for a single app, or for all of them
            //TODO: use AssociationCache in the future
            var query = app ? entities.AssociationData.maxRowstampQueries.fmt(app) : entities.AssociationData.maxRowstampQueries;
            swdbDAO.findByQuery('AssociationData', null, { fullquery: query })
                .then(function (queryResults) {
                    var result = {};
                    var associationmap = {};
                    for (var i = 0; i < queryResults.length; i++) {
                        var item = queryResults[i];
                        associationmap[item.application] = { "maximorowstamp": item.rowstamp }
                    }
                    var end = new Date().getTime();
                    log.debug("generated rowstampmap for associations. Ellapsed {0} ms".format(end - start));
                    result.associationmap = associationmap;
                    deferred.resolve(result);
                });
            return deferred.promise;
        }



    }
});