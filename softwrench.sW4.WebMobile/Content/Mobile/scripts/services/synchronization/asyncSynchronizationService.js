(function (angular) {
    "use strict";

    var AsyncSynchronizationService = function ($q, swdbDAO, restService) {

        var batchRegistry = {};
        var completionCallBacks = [];
        var pollingDelay = 60 * 1000; // TODO: get from config

        var getBatchIds = function() {
            var ids = [];
            for (var id in batchRegistry) {
                if (!batchRegistry.hasOwnProperty(id)) continue;
                ids.push(id);
            }
            return ids;
        };

        setInterval(function () {
            var ids = getBatchIds();
            if (ids.length <= 0 || completionCallBacks.length <= 0) return;

            var start = new Date().getTime();
            restService.postPromise("Mobile", "BatchStatus", null, { ids: ids })
                .then(function(response) {
                    return response.data;
                })
                .then(function(batches) {
                    var batchTuples = [];
                    angular.forEach(batches, function(batch) {
                        if (batch.status !== "COMPLETE") return;
                        var localBatch = batchRegistry[batch.remoteId];
                        batchTuples.push({ remote: batch, local: localBatch });
                    });
                    return batchTuples;
                })
                .then(function(batchTuples) {
                    if (batchTuples.length <= 0) {
                        return;
                    }
                    angular.forEach(completionCallBacks, function(callback) {
                        callback({ start: start, batchTuples: batchTuples });
                    });
                    angular.forEach(batchTuples, function(tuple) {
                        delete batchRegistry[tuple.local.id];
                    });
                });

        }, pollingDelay);

        this.onBatchesCompleted = function(callback) {
            completionCallBacks.push(callback);
        };

        this.registerForAsyncProcessing = function (batch) {
            if (batchRegistry[batch.id]) {
                return;
            }
            batchRegistry[batch.id] = batch;
        };

    };

    angular.module("softwrench").service("asyncSynchronizationService", ["$q", "swdbDAO", "restService", AsyncSynchronizationService]);

})(angular);