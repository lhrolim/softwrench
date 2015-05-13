mobileServices.factory('batchService', function ($q, restService, swdbDAO, $log) {

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
                if (batch != null) {
                    promises.push(restService.postPromise('Mobile', 'SubmitBatch', batch.items));
                }
            }
            if (promises.length == 0) {
                log.info("no batches submitted");
                return $q.when();
            }
            return $q.all(promises);
        },


        createBatch: function (application) {

            var log = $log.getInstance('batchService#createBatch');

            return swdbDAO.findByQuery('DataEntry', "isDirty = 1 and application = '{0}'".format(application))
                .then(function (items) {
                    if (items.length == 0) {
                        log.debug('no items to submit to the server. returning null batch');
                        //nothing to do, interrupting chain
                        return $q.reject();
                    }
                    var batchItemPromises = [];
                    var batchDeletePromises = [];
                    for (var i = 0; i < items.length; i++) {
                        batchItemPromises.push(swdbDAO.instantiate('BatchItem', items[i]));
                        batchDeletePromises.push($q.when(item));
                    }
                    var txPromise = swdbDAO.createTx();
                    var batchPromise = swdbDAO.instantiate('Batch');
                    log.debug('creating db promises');
                    return $q.all([txPromise, batchPromise, batchItemPromises, batchDeletePromises]);
                }).catch(function (err) {
                    return $q.reject(err);
                }).then(function (items) {
                    var tx = items[0];
                    var batch = items[1];
                    var batchItemsToCreate = items[2];
                    var dataEntriesToDelete = items[3];
                    batch.application = application;
                    batch.sentDate = new Date();
                    for (var i = 0; i < batchItemsToCreate.length; i++) {
                        //creating relationships
                        var item = batchItemsToCreate[i];
                        batch.items.add(item);
                        item.batch = batch;
                    }
                    //use a single transaction here
                    swdbDAO.bulkSave(batchItemsToCreate, tx);
                    swdbDAO.bulkDelete(dataEntriesToDelete, tx);
                    swdbDAO.save(batch, tx);
                    try {
                        log.debug('executing batching db tx');
                        persistence.flush(tx, function () {
                            $q.when(batch);
                        });
                    } catch (err) {
                        $q.reject(err);
                    }
                }).catch(function (error) {
                    if (!error) {
                        //it was interrupted due to an abscence of items, but it should resolve to the outer calls!
                        return $q.when(null);
                    }
                    return $q.reject();
                });
        }

    }

})
