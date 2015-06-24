(function() {
    'use strict';

    service.$inject = ['$q', 'restService', 'swdbDAO', '$log', 'schemaService', 'offlineSchemaService'];


    mobileServices.factory('batchService', service);


    function service($q, restService, swdbDAO, $log, schemaService, offlineSchemaService) {

        var api = {
            getIdsFromBatch: getIdsFromBatch,
            submitBatches: submitBatches,
            generateDatamapDiff: generateDatamapDiff,
            createBatch: createBatch
        };

        return api;

        function getIdsFromBatch(batch) {
            var items = batch.items;
            var ids = [];
            for (var i = 0; i < items.length; i++) {
                var batchItem = items[i];
                ids.push(batchItem['remoteId']);
            }
            return ids;
        };

        /**
         * Executes a request to submit a Batch and handles the returned response.
         * 
         * @param Batch batch 
         * @param [Object] items to be sent as payload
         * @returns Promise: resolved with updated Batch; rejected with Http or Database error 
         */
        function doSubmitBatch(batch, items) {
            var log = $log.getInstance('batchService#doSubmitBatch');
            // preparing the request
            var batchParams = {
                application: batch.application,
                remoteId: batch.id
            }
            var jsonToSend = angular.toJson({ items : items });
            // performing the request
            log.info("Submitting a Batch (id='{0}') to the server.".format(batch.id));
            return restService
                .postPromise("Mobile", "SubmitBatch", batchParams, jsonToSend)
                .then(function (response) {
                    var returnedBatch = response.data;
                    var returnedBatchStatus = returnedBatch.status;
                    batch.status = returnedBatchStatus; // always update status
                    var indexedItems = {}; // items indexed by their id
                    batch.loadeditems.forEach(function (item) {
                        // status update
                        item.status = returnedBatchStatus;
                        // indexing items
                        indexedItems[item.id] = item;
                    });
                    log.info("Batch response received (id='{0}') with Batch.status = '{1}'".format(batch.id, returnedBatchStatus));
                    // assynchronous case: awaiting to be processed
                    if (returnedBatchStatus !== "COMPLETE") {
                        // update batch status
                        // TODO: register for polling
                        return saveBatch(batch, batch.loadeditems);
                    }
                    // synchronous case: request already processed
                    // has to update with problems and/or success
                    var problems = returnedBatch.problems; // indexed by related batchItem.id
                    var problemEntities = [];
                    for (var itemId in problems) {
                        if (!problems.hasOwnProperty(itemId)) continue;
                        // instantiate Problem (synchronously actually helps in this case)
                        var problem = problems[itemId];
                        var problemEntity = new entities.Problem();
                        problemEntity.message = problem.message;
                        // item pointing to Problem and status update
                        var problematicItem = indexedItems[itemId.toUpperCase()]; // uppercasing in case the server camelcased the keys
                        problematicItem.problem = problemEntity;
                        // add to array
                        problemEntities.push(problemEntity);
                    }
                    // save problems and update batch status
                    if (problemEntities.length > 0) {
                        return saveBatch(batch, batch.loadeditems, problemEntities);
                    }
                    // just update statuses
                    return saveBatch(batch, batch.loadeditems);
                });
        };

        function submitBatches(batches) {

            var log = $log.getInstance('batchService#submitBatches');

            var promises = []; // parallel requests promises
            batches.forEach(function (batch) {
                if (!batch || !batch.loadeditems) {
                    return;
                }
                var items = batch.loadeditems.map(function(batchItem) {
                    return {
                        datamap: generateDatamapDiff(batchItem),
                        itemId: batchItem.dataentry.remoteId,
                        //local id becomes remote from the server perspective
                        remoteId: batchItem.id,
                        application: batch.application
                    };
                });
                // put the batch submission promise in the array
                promises.push(doSubmitBatch(batch, items));
            });

            // no batches were submitted: reject it
            if (promises.length <= 0) {
                log.info("no batches submitted");
                return $q.when();
            }

            // joined promises: resolves with array of Batch
            return $q.all(promises);
        };

        function generateDatamapDiff(batchItem) {
            if (batchItem.operation) {
                return batchItem.operation.datamap;
            }
            var datamap = batchItem.dataentry.datamap;
            var original = batchItem.dataentry.originalDatamap;

            //TODO: implement the diff, passing also the ID + siteid all the time
            return datamap;

        };

        /**
         * Saves a Batch it's children BatchItems and another possible 
         * list of a related entity (such as DataEntry or Problem) in a single transaction.
         * This method requires all parameters to be currently in a persistence "managed" state
         * i.e. they need to be actual entity instances, it won't work with "simple" objects/dictionaries.
         * 
         * @param Batch batch 
         * @param [BatchItem] batchItems (optional) 
         * @param [persistence.Entity] managedEntities (optional)
         * @returns Promise: resolved with the saved batch; rejected with Database error
         */
        function saveBatch(batch, batchItems, managedEntities) {
            var log = $log.getInstance("batchService#saveBatch");
            var deferred = $q.defer();
            persistence.transaction(function (tx) {
                log.debug("executing batching db tx");
                /* In this configuration: results[n].dataentry.datamap is always null
                var dbPromise = managedEntities ? swdbDAO.bulkSave(managedEntities) : $q.when();
                dbPromise.then(function() {
                    return batchItems ? swdbDAO.bulkSave(batchItems) : $q.when();
                }).then(function() {
                    return swdbDAO.save(batch);
                }).then(function(savedBatch) {
                    savedBatch.items.list(null, function (results) {
                        savedBatch.loadeditems = results;
                        deferred.resolve(savedBatch);
                    });
                }).catch(function(err) {
                    deferred.reject(err);
                });
                */
                /* in this configuration: results[n].problem is always null
                swdbDAO.save(batch)
                    .then(function () {
                        return managedEntities ? swdbDAO.bulkSave(managedEntities) : $q.when();
                    })
                    .then(function () {
                        return batchItems ? swdbDAO.bulkSave(batchItems) : $q.when();
                    })
                    .then(function() {
                        batch.items.list(null, function (results) {
                            batch.loadeditems = results;
                            deferred.resolve(batch);
                        });
                    }).catch(function(err) {
                        deferred.reject(err);
                    });*/

                // in this configuration: not fetching list from db, setting from memory
                swdbDAO.save(batch)
                    .then(function(savedBatch) {
                        return managedEntities ? swdbDAO.bulkSave(managedEntities) : $q.when();
                    })
                    .then(function(savedEntities) {
                        return batchItems ? swdbDAO.bulkSave(batchItems) : $q.when();
                    })
                    .then(function(items) {
                        if (items) {
                            batch.loadeditems = items;
                        };
                        deferred.resolve(batch);
                    }).catch(function(err) {
                        deferred.reject(err);
                    });
            });
            return deferred.promise;
        }


        function createBatch(dbapplication) {
            var applicationName = dbapplication.application;

            var log = $log.getInstance('batchService#createBatch');

            var detailSchema = offlineSchemaService.locateSchemaByStereotype(dbapplication, "detail");

            return swdbDAO.findByQuery('DataEntry', "isDirty = 1 and pending=0 and application = '{0}'".format(applicationName))
                .then(function (items) {
                    if (items.length <= 0) {
                        log.debug('no items to submit to the server. returning null batch');
                        //nothing to do, interrupting chain
                        return $q.reject();
                    }
                    var batchItemPromises = [];
                    var length = items.length;
                    for (var i = 0; i < length; i++) {
                        var entry = items[i];
                        entry.pending = true;
                        entry.isDirty = false;
                        batchItemPromises.push(swdbDAO.instantiate('BatchItem', entry, function (dataEntry, batchItem) {
                            batchItem.dataentry = dataEntry;
                            batchItem.status = 'pending';
                            batchItem.label = schemaService.getTitle(detailSchema, dataEntry.datamap, true);
                            batchItem.crudoperation = dataEntry.crudoperation;
                            return batchItem;
                        }));
                    }
                    var batchPromise = swdbDAO.instantiate('Batch');
                    log.debug('creating db promises');
                    var dbPromises = [];
                    dbPromises.push(batchPromise);
                    dbPromises.push($q.when(items));
                    dbPromises = dbPromises.concat(batchItemPromises);
                    return $q.all(dbPromises);
                }).catch(function (err) {
                    return $q.reject(err);
                }).then(function (items) {
                    var batch = items[0];
                    var dataEntries = items[1];
                    var batchItemsToCreate = items.subarray(2, length + 1);
                    batch.application = applicationName;
                    batch.sentDate = new Date();

                    for (var i = 0; i < batchItemsToCreate.length; i++) {
                        //creating relationships
                        var item = batchItemsToCreate[i];
                        batch.items.add(item);
                        item.batch = batch;
                    }
                    return saveBatch(batch, batchItemsToCreate, dataEntries);
                }).catch(function (error) {
                    if (!error) {
                        //it was interrupted due to an abscence of items, but it should resolve to the outer calls!
                        return $q.when();
                    }
                    return $q.reject();
                });
        }


    }
})();





