mobileServices.factory('batchService', function ($q, restService, swdbDAO, $log, schemaService, offlineSchemaService) {

    return {

        getIdsFromBatch: function (batch) {
            var items = batch.items;
            var ids = [];
            for (var i = 0; i < items.length; i++) {
                var batchItem = items[i];
                ids.push(batchItem['remoteId']);
            }
            return ids;
        },

        submitBatches: function (batches) {

            var log = $log.getInstance('batchService#submitBatches');
            var promises = [];
            for (var i = 0; i < batches.length; i++) {
                var batch = batches[i];
                if (batch == null) {
                    continue;
                }
                var batchParams = {
                    application: batch.application,
                    remoteId: batch.id
                }
                var itemArray = [];
                var loadeditems = batch.loadeditems;
                for (var j = 0; j < loadeditems.length; j++) {
                    var batchItem = loadeditems[j];
                    var item = {
                        datamap: this.generateDatamapDiff(batchItem),
                        itemId: batchItem.dataentry.remoteId,
                        //local id becomes remote from the server perspective
                        remoteId: batchItem.id,
                        application: batch.application
                    };
                    itemArray.push(item);
                }
                var jsonToSend = { items: itemArray };
                promises.push(restService.postPromise('Mobile', 'SubmitBatch', batchParams, angular.toJson(jsonToSend)));
            }
            if (promises.length == 0) {
                log.info("no batches submitted");
                return $q.when();
            }
            return $q.all(promises);
        },

        generateDatamapDiff: function (batchItem) {
            if (batchItem.operation) {
                return batchItem.operation.datamap;
            }
            var datamap = batchItem.dataentry.datamap;
            var original = batchItem.dataentry.originalDatamap;

            //TODO: implement the diff, passing also the ID + siteid all the time
            return datamap;

        },


        createBatch: function (dbapplication) {
            var applicationName = dbapplication.application;

            var log = $log.getInstance('batchService#createBatch');

            var detailSchema = offlineSchemaService.locateSchemaByStereotype(dbapplication, "detail");

            return swdbDAO.findByQuery('DataEntry', "isDirty = 1 and pending=0 and application = '{0}'".format(applicationName))
                .then(function (items) {
                    if (items.length == 0) {
                        log.debug('no items to submit to the server. returning null batch');
                        //nothing to do, interrupting chain
                        return $q.reject();
                    }
                    var batchItemPromises = [];
                    var length = items.length;
                    for (var i = 0; i < length; i++) {
                        var entry = items[i];
                        batchItemPromises.push(swdbDAO.instantiate('BatchItem', entry, function (dataEntry, batchItem) {
                            batchItem.dataentry = dataEntry;
                            batchItem.status = 'pending';
                            batchItem.label = schemaService.getTitle(detailSchema, dataEntry.datamap, true);
                            return batchItem;
                        }));
                        entry.pending = true;
                        entry.isDirty = false;
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
                    var deferred = $q.defer();
                    persistence.transaction(function (tx) {
                        try {
                            log.debug('executing batching db tx');
                            swdbDAO.bulkSave(batchItemsToCreate, tx);
                            swdbDAO.bulkSave(dataEntries, tx);
                            swdbDAO.save(batch, tx);
                            persistence.flush(tx, function () {
                                batch.items.list(null, function (results) {
                                    batch.loadeditems = results;
                                    return deferred.resolve(batch);
                                });
                            });
                        } catch (err) {
                            deferred.reject(err);
                        }
                    });
                    return deferred.promise;
                    //use a single transaction here
                }).catch(function (error) {
                    if (!error) {
                        //it was interrupted due to an abscence of items, but it should resolve to the outer calls!
                        return $q.when();
                    }
                    return $q.reject();
                });
        }

    }

})
