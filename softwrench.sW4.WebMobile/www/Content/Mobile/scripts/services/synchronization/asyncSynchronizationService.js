(function (mobileServices, angular) {
    "use strict";

    /**
     * @constructor 
     */
    var AsyncSynchronizationService = function ($q, restService, $log, swdbDAO, $interval, $rootScope) {

        // Batches indexed by their id
        var batchRegistry = {};
        // list of callbacks to be called on Batch completion
        var completionCallBacks = [];
        // polling interval in milliseconds
        var pollingDelay = 60 * 1000; // TODO: get from config

        var self = this;

        // clear batchRegistry (only user related state) on logout
        $rootScope.$on("sw4:security:logout", function() {
            batchRegistry = {};
        });

        var getBatchIds = function() {
            var ids = [];
            for (var id in batchRegistry) {
                if (!batchRegistry.hasOwnProperty(id)) {
                    continue;
                }
                ids.push(id);
            }
            return ids;
        };

        var fetchBatchStatus = function() {
            var log = $log.get("asyncSynchronizationService#fetchBatchStatus");
            var ids = getBatchIds();
            if (ids.length <= 0 || completionCallBacks.length <= 0) {
                return;
            }
            var now = new Date();
            log.info("Polling request started at {0}".format(now));
            restService.get("Mobile", "BatchStatus", { ids: ids })
                .then(function (response) {
                    log.info("Polling response received");
                    return response.data;
                })
                .then(function (batches) {
                    var batchTuples = [];
                    angular.forEach(batches, function (batch) {
                        if (batch.status !== "COMPLETE") return;
                        var localBatch = batchRegistry[batch.remoteId];
                        batchTuples.push({ remote: batch, local: localBatch });
                    });
                    return batchTuples;
                })
                .then(function (batchTuples) {
                    if (batchTuples.length <= 0) {
                        log.info("no 'COMPLETE' batches received");
                        return;
                    }
                    angular.forEach(completionCallBacks, function (callback) {
                        callback({ start: now.getTime(), batchTuples: batchTuples });
                    });
                    angular.forEach(batchTuples, function (tuple) {
                        delete batchRegistry[tuple.local.id];
                    });
                })
                .catch(function (error) {
                    log.error(error);
                });
        };

        /**
         * Registers a Batch to be asynchronously processed i.e. it's "status" will be fetched asynchronously from the server.
         * If the Batch was already registered it won't be registered again (checks equality by Batch#id).
         * Since it's an assynchronous process callback functions will be called once it's status are fetched.
         * The callbacks can be registered in {@link AsyncSynchronizationService#onBatchesCompleted}.
         * 
         * @param Batch batch 
         */
        this.registerForAsyncProcessing = function(batch) {
            if (batchRegistry[batch.id]) {
                return;
            }
            batchRegistry[batch.id] = batch;
        };

        /**
         * Registers a list of Batches to be asynchronously processed.
         * Calls {@link AsyncSynchronizationService#registerForAsyncProcessing} on each element of the list
         * 
         * @param [Batch] batches 
         */
        this.registerListForAsyncProcessing = function(batches) {
            angular.forEach(batches, function(batch) {
                self.registerForAsyncProcessing(batch);
            });
        };

        /**
         * Register/add a callback to be called when a Batch status response is received.
         * The callback receives a single object as parameter:
         * <pre>
         * {
         *  "start": Number, // timestamp of when the request was made
         *  "batchTuples": [{
         *      "local": Batch, // Batch entity that was registered for async processing
         *      "remote": Object // Batch response from the server
         *  }]
         *  }
         * </pre>
         * Note that the callback receives an array of Batches instead of a single Batch. 
         * That's because this client requests the status of multiple Batches at once in order to 
         * optimize network usage.
         * Also the callback will only be called passing the Batches with status "COMPLETE" i.e. 
         * there's no need for the caller to worry about re-registering a non-complete Batch for async processing,
         * only complete Batches will be sent to the callbacks and non-complete ones will remain registered to 
         * await further completion.
         * 
         * @param Function callback 
         */
        this.onBatchesCompleted = function (callback) {
            completionCallBacks.push(callback);
        };

        /**
         * Fills the batchRegistry with initial data.
         */
        var loadBatchesForProcessing = function () {
            var ids = getBatchIds();
            ids = ids.map(function(id) {
                return "'{0}'".format(id);
            });
            var query = "status!='COMPLETE'";
            if (ids.length > 0) {
                query += " and id not in ({0})".format(ids);
            }
            swdbDAO.findByQuery("Batch", query)
                .then(function(batches) {
                    var promises = [];
                    angular.forEach(batches, function (batch) {
                        var deferred = $q.defer();
                        batch.items.prefetch("dataentry").prefetch("problem").list(null, function(items) {
                            batch.loadeditems = items;
                            deferred.resolve(batch);
                        });
                        promises.push(deferred.promise);
                    });
                    return $q.all(promises);
                })
                .then(function (batches) {
                    self.registerListForAsyncProcessing(batches);
                });
        };

        var startPolling = function () {
            $interval(fetchBatchStatus, pollingDelay);
        };

        loadBatchesForProcessing();
        startPolling();

    };

    mobileServices.service("asyncSynchronizationService", ["$q", "offlineRestService", "$log", "swdbDAO", "$interval", "$rootScope", AsyncSynchronizationService]);

})(mobileServices, angular);
