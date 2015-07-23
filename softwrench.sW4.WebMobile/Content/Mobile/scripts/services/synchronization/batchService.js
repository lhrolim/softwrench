﻿(function (mobileServices, angular, persistence) {
    'use strict';

    function service($q, restService, swdbDAO, $log, schemaService, offlineSchemaService, operationService) {

        //#region Utils

        /**
         * internal rollback context
         */
        var rollbackContext = {
            // Queue: DataEntry instances currently being submitted to the server
            submittingEntries: []
        };

        var logIdsIfEnabled = function (logger, level, entities, messageTemplate) {
            // just so it doesn't waste time and memory creating a message that won't be logged
            if (!logger.isLevelEnabled(level)) {
                return;
            }
            var ids = entities.map(function (e) {
                return "'{0}'".format(e.id);
            });
            logger[level](messageTemplate.format(ids));
        };

        /**
         * Adds the dataentry to the rollbackcontext.
         * 
         * @param [DataEntry] dataentry either an array of entries or a single entry
         */
        var addToRollbackContext = function (dataentry) {
            var log = $log.get("BatchService#rollback");
            if (angular.isArray(dataentry)) {
                logIdsIfEnabled(log, "debug", dataentry, "marking DataEntries {0} to rollback");
                angular.forEach(dataentry, function(e) {
                    rollbackContext.submittingEntries.push(e);
                });
            } else {
                log.debug("marking DataEntry '{0}' to rollback".format(dataentry.id));
                rollbackContext.submittingEntries.push(dataentry);
            }
        };

        /**
         * Updates the entries added to the rollbackcontext
         * setting their pending property to true.
         * 
         * (?) If the rollbackk mechanism fails for any reason the entries 
         * will remain in the rollbackcontext. (?)
         */
        var rollBackSubmittingDataEntries = function () {
            var log = $log.get("BatchService#rollback");
            log.debug("executing rollback");
            var nonPendingEntries = [];
            while (rollbackContext.submittingEntries.length > 0) {
                // evicting entries from internal state
                var entry = rollbackContext.submittingEntries.shift();
                if (!entry) continue;
                // mark as not pending
                entry.pending = false;
                // pushing to this function scoped state
                nonPendingEntries.push(entry);
            }
            swdbDAO.bulkSave(nonPendingEntries)
                .then(function (entries) {
                    logIdsIfEnabled(log, "debug", entries, "DataEntries {0} rolledback successfully");
                }).catch(function (error) {
                    logIdsIfEnabled(log, "warn", nonPendingEntries, "Failed to rollback DataEntries {0} due to " + error);
                    // (?) addToRollbackContext(nonPendingEntries); (?)
                });
        };

        /**
         * Empties the rollbackcontext.
         * 
         * (?) Maybe do a per-entry eviction: only remove those passed as arguments. 
         * Useful if we decide to maintain the entries in the context if their rollback fail (?)
         */
        var evictRollBackContext = function() {
            rollbackContext.submittingEntries = [];
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

        //#endregion

        //#region Public methods

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
            return $q.all(promises)
                .then(function (results) {
                    // entries can be removed from rollbackcontext
                    evictRollBackContext();
                    return results;
                })
                .catch(function (error) {
                    // rollback DataEntries update
                    rollBackSubmittingDataEntries();
                    // still reject the error to communicate it along the promise chain
                    return $q.reject(error);
                });
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
                        // if batch submission fails, remember to rollback this state (otherwise dataentry will be in limbo)
                        entry.pending = true;
                        addToRollbackContext(entry);
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

        //#endregion
        
        //#region Service instance

        var api = {
            getIdsFromBatch: getIdsFromBatch,
            submitBatches: submitBatches,
            generateDatamapDiff: generateDatamapDiff,
            createBatch: createBatch,
            saveBatch: saveBatch,
            updateBatch: updateBatch,
        };

        return api;

        //#endregion
    };
    
    //#region Service registration
    mobileServices.factory('batchService', ['$q', 'restService', 'swdbDAO', '$log', 'schemaService', 'offlineSchemaService', 'operationService', service]);
    //#endregion

})(mobileServices, angular, persistence);





