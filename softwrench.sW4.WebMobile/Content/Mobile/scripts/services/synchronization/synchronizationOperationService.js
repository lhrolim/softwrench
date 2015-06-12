(function() {
"use strict";

modules.webcommons.factory("synchronizationOperationService", ['$log', '$q', 'swdbDAO',
    function($log, $q, swdbDAO) {

        var internalListContext = {
            lastPageLoaded: 1
        }

        var context = {
            list: null
        }


        return {
            createBatchOperation: function(startdate, relatedBatches) {
                var log = $log.get("synchronizationOperationService#createBatchOperation");
                var operation = {
                    startdate: startdate,
                    status: "PENDING"
                };
                // return promise chain initiated with the creation of SyncOperation
                return swdbDAO.instantiate("SyncOperation", operation)
                    .then(function(operationEntity) {
                        relatedBatches.forEach(function (batch) {
                            operationEntity.batches.add(batch);
                            batch.syncoperation = operationEntity;
                        });
                        var deferred = $q.defer();
                        persistence.transaction(function(tx) {
                            try {
                                swdbDAO.bulkSave(relatedBatches, tx);
                                swdbDAO.save(operationEntity, tx);
                                // flushing transaction
                                persistence.flush(tx, function() {
                                    // resolve promise with the syncOperation
                                    deferred.resolve(operationEntity);
                                });
                            } catch (error) {
                                // reject promise with DB error
                                deferred.reject(error);
                            }
                        });
                        return deferred.promise;
                    });
            },

            createNonBatchOperation: function(startdate, enddate, numberofdownloadeditems, numberofdownloadedsupportdata, metadatachange) {
                var operation = {
                    startdate: startdate,
                    enddate: enddate,
                    status: 'COMPLETE',
                    numberofdownloadeditems: numberofdownloadeditems,
                    numberofdownloadedsupportdata: numberofdownloadedsupportdata,
                    metadatachange: metadatachange,
                };
                return swdbDAO.instantiate('SyncOperation', operation).then(function(item) {
                    return swdbDAO.save(item);
                });
            },

            refreshSync: function() {
                context.list = null;
            },

            hasProblems: function() {
                //TODO:implement
                return false;
            },

            getSyncList: function() {
                if (context.list) {
                    return context.list;
                }
                context.list = [];
                return swdbDAO.findByQuery('SyncOperation', "", { pagesize: 10, pagenumber: internalListContext.pageNumber, orderby: 'startdate', orderbyascending: false }).then(function(results) {
                    context.list = results;
                    return $q.when();
                });
            }

        }

    }
]);

})();