(function () {
    'use strict';

    service.$inject = ['$q', 'restService', 'swdbDAO', '$log', 'schemaService', 'offlineSchemaService', 'operationService'];


    mobileServices.factory('batchService', service);


    function service($q, restService, swdbDAO, $log, schemaService, offlineSchemaService, operationService, asyncSynchronizationService) {

        var api = {
            getIdsFromBatch: getIdsFromBatch,
            submitBatches: submitBatches,
            generateDatamapDiff: generateDatamapDiff,
            createBatch: createBatch,
            saveBatch: saveBatch,
            updateBatch: updateBatch,
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
                //save managed entities before the batchItems so that their properties are not null for loaded items
                if (managedEntities) swdbDAO.bulkSave(managedEntities, tx);
                if (batchItems) swdbDAO.bulkSave(batchItems, tx);
                swdbDAO.save(batch, tx);
                persistence.flush(tx, function () {
                    batch.items.list(null, function (results) {
                        batch.loadeditems = results;
                        return deferred.resolve(batch);
                    });
                });
            });
            return deferred.promise;
        }

        /**
         * Updates a local Batch entity according to a Batch response from the server.
         * The updates includes: updating related BatchItem's problems, updating related DataEntries's flags
         * and updating status of the Batch and the realted entities. 
         * The method also checks if the remoteBatch is a synchronous/asynchronous response 
         * and takes the correct action flow.
         * 
         * @param Batch batch entity 
         * @param Object remoteBatch Batch response from server 
         * @returns Promise: resolved with the updated Batch. 
         */
        function updateBatch(batch, remoteBatch) {
            var log = $log.get("batchService#updateBatch");
            var returnedBatchStatus = remoteBatch.status;
            batch.status = returnedBatchStatus; // always update status
            var indexedItems = {}; // items indexed by their id
            batch.loadeditems.forEach(function (item) {
                // status update
                item.status = returnedBatchStatus;
                // indexing items
                indexedItems[item.id] = item;
            });
            log.info("Batch response received (id='{0}') with Batch.status = '{1}'".format(remoteBatch.remoteId, returnedBatchStatus));
            // assynchronous case: awaiting to be processed
            if (returnedBatchStatus !== "COMPLETE") {
                // update batch status
                return saveBatch(batch, batch.loadeditems);
            }
            // synchronous case: request already processed
            // has to update with problems and/or success
            var problems = remoteBatch.problems; // indexed by related batchItem.id
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
            var successItems = remoteBatch.successItems;
            angular.forEach(successItems, function(successId) {
                var successItem = indexedItems[successId.toUpperCase()]; // uppercasing in case the server camelcased the keys
                if (successItem.problem) successItem.problem = null; // problem shouldn't be deleted for history purposes
            });
            // update items's DataEntries's flags
            batch.loadeditems.forEach(function (item) {
                item.dataentry.pending = false;
                item.dataentry.isDirty = !!item.problem;
            });
            // save problems, update statuses and flags
            var hasProblems = problemEntities.length;
            batch.hasProblems = hasProblems;
            if (hasProblems > 0) {
                return saveBatch(batch, batch.loadeditems, problemEntities);
            }
            // no problems found: just update statuses and flags
            return saveBatch(batch, batch.loadeditems);
        }

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
            var jsonToSend = angular.toJson({ items: items });
            // performing the request
            log.info("Submitting a Batch (id='{0}') to the server.".format(batch.id));
            return restService
                .postPromise("Mobile", "SubmitBatch", batchParams, jsonToSend)
                .then(function (response) {
                    var returnedBatch = response.data;
                    return updateBatch(batch, returnedBatch);
                });
        };

        function submitBatches(batches) {

            var log = $log.getInstance('batchService#submitBatches');

            var promises = []; // parallel requests promises
            batches.forEach(function (batch) {
                if (!batch || !batch.loadeditems) {
                    return;
                }
                var items = batch.loadeditems.map(function (batchItem) {
                    return {
                        datamap: generateDatamapDiff(batchItem),
                        itemId: batchItem.dataentry.remoteId,
                        //local id becomes remote from the server perspective
                        remoteId: batchItem.id,
                        application: batch.application,
                        operation: batchItem.crudoperation
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

        


        function createBatch(dbapplication) {
            var applicationName = dbapplication.application;

            var log = $log.getInstance('batchService#createBatch');

            var detailSchema = offlineSchemaService.locateSchemaByStereotype(dbapplication, "detail");

            return swdbDAO.findByQuery('DataEntry', "isDirty=1 and pending=0 and application='{0}'".format(applicationName))
                .then(function (dataEntries) {
                    if (!dataEntries || dataEntries.length <= 0) {
                        log.debug("no items to submit to the server. returning null batch");
                        return null;
                    }
                    var batchItemPromises = [];
                    angular.forEach(dataEntries, function (entry) {
                        entry.pending = true;
                        batchItemPromises.push(swdbDAO.instantiate('BatchItem', entry, function (dataEntry, batchItem) {
                            batchItem.dataentry = dataEntry;
                            batchItem.status = 'pending';
                            batchItem.label = schemaService.getTitle(detailSchema, dataEntry.datamap, true);
                            batchItem.crudoperation = operationService.getCrudOperation(dataEntry);
                            return batchItem;
                        }));
                    });
                    var dbPromises = [];
                    log.debug('creating db promises');
                    var batchPromise = swdbDAO.instantiate('Batch');
                    dbPromises.push(batchPromise);
                    dbPromises.push($q.when(dataEntries));
                    dbPromises = dbPromises.concat(batchItemPromises);
                    return $q.all(dbPromises);
                }).then(function (items) {
                    if (!items) {
                        return items;
                    }
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
                });
        }


    }
})();





