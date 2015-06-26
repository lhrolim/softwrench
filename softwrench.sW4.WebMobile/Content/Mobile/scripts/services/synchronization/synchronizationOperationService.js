(function() {
    "use strict";

    /**
     * Service contructor funtion.
     * 
     * @param {} $log 
     * @param {} $q 
     * @param {} swdbDAO 
     * @returns {} service instance
     * @constructor 
     */
    var SynchronizationOperationService = function ($log, $q, swdbDAO, formatter) {

        var internalListContext = {
            lastPageLoaded: 1
        }

        var saveBatchOperation = function (operation, relatedBatches) {
            return swdbDAO.instantiate("SyncOperation", operation)
                .then(function (operationEntity) {
                    // all batches need to be complete in order for the syncoperation to be complete
                    var isComplete = relatedBatches.every(function (batch) {
                        return batch.status === "COMPLETE";
                    });
                    // add relationships
                    relatedBatches.forEach(function (batch) {
                        operationEntity.batches.add(batch);
                        if (batch.loadeditems && batch.loadeditems !== null && batch.loadeditems.length > 0) {
                            operationEntity.items += batch.loadeditems.length;
                        }
                        batch.syncoperation = operationEntity;
                    });
                    if (isComplete) {
                        var hasProblem = relatedBatches.some(function (result) {
                            //if every batch returned as complete than we have a synchronous case and can close the sync operation
                            return result.hasProblems;
                        });
                        operationEntity.status = "COMPLETE";
                        operationEntity.enddate = new Date().getTime();
                        operationEntity.hasProblems = hasProblem;
                    } else {
                        operationEntity.status = "PENDING";
                    }
                    var deferred = $q.defer();
                    persistence.transaction(function (tx) {
                        try {
                            swdbDAO.bulkSave(relatedBatches, tx);
                            swdbDAO.save(operationEntity, tx);
                            // flushing transaction
                            persistence.flush(tx, function () {
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
        }

        this.createBatchOperation = function(startdate, relatedBatches) {
            var operation = {
                startdate: startdate,
                items: 0
            };
            // return promise chain initiated with the creation of SyncOperation
            return saveBatchOperation(operation, relatedBatches);
        };

        this.createSynchronousBatchOperation = function (startdate, numberofdownloadeditems, relatedBatches) {

            var hasProblem = relatedBatches.some(function (result) {
                //if every batch returned as complete than we have a synchronous case and can close the sync operation
                return result.hasProblems;
            });

            var operation = {
                startdate: startdate,
                numberofdownloadeditems: numberofdownloadeditems,
                numberofdownloadedsupportdata: 0,
                hasProblems: hasProblem
            };
            return saveBatchOperation(operation, relatedBatches);
        };

        this.createNonBatchOperation = function(startdate, enddate, numberofdownloadeditems, numberofdownloadedsupportdata, metadatachange) {
            var operation = {
                startdate: startdate,
                enddate: enddate,
                status: "COMPLETE",
                numberofdownloadeditems: numberofdownloadeditems,
                numberofdownloadedsupportdata: numberofdownloadedsupportdata,
                metadatachange: metadatachange
            };
            return swdbDAO.instantiate("SyncOperation", operation).then(function(item) {
                return swdbDAO.save(item);
            });
        }

        this.hasProblems = function (operation) {
            return operation.hasProblems;
        };

        this.getSyncList = function () {
            var self = this;
            return swdbDAO.findByQuery("SyncOperation", null, { pagesize: 10, pagenumber: internalListContext.pageNumber, orderby: "startdate", orderbyascending: false })
                .then(function(operations) {
                    if (!operations || operations.length <= 0) {
                        return operations;
                    }
                    // format the operations found
                    return operations.map(function(operation) {
                        return self.formatOperation(operation);
                    });
                });
        };

        /**
         * Fetches the SyncOperation the has the matching id 
         * 
         * @param int id primary key
         * @returns Promise: resolved with SyncOperation entity, rejected with database Error. 
         */
        this.getOperation = function (id) {
            var self = this;
            return swdbDAO.findById("SyncOperation", id)
                .then(function(operation) {
                    return self.formatOperation(operation);
                });
        };

        this.doneNoProblems = function(operation) {
            return operation.status.equalIc("complete") && !this.hasProblems(operation);
        };

        this.doneWithProblems = function(operation) {
            return operation.status.equalIc("complete") && this.hasProblems(operation);
        };

        this.isPending = function(operation) {
            return operation.status.equalIc("pending");
        };

        /**
         * Adds a few extra fields to the operation:
         * <ul>
         * <li>'formatteddate': String - formatted startdate </li>
         * <li>'formattedstatus': String - "displayable" status </li>
         * <li>'issuccess': Boolean </li>
         * <li>'ispending': Boolean </li>
         * <li>'hasproblems': Boolean </li>
         * </ul>
         * @param SyncOperation (entity) operation 
         * @returns SyncOperation (entity) formatted 
         */
        this.formatOperation = function (operation) {
            if (!operation) {
                return operation;
            }
            operation.formatteddate = formatter.formatDate(operation.startdate, "MM/dd/yyyy HH:mm");
            operation.elapsedtime = ((operation.enddate - operation.startdate) / 1000);

            if (this.doneNoProblems(operation)) {
                operation.formattedstatus = "Completed";
                operation.issuccess = true;
            } else if (this.isPending(operation)) {
                operation.formattedstatus = "Pending";
                operation.ispending = true;
            } else if (this.doneWithProblems(operation)) {
                operation.formattedstatus = "Completed with Issues";
                operation.hasproblems = true;
            }
            return operation;
        };

        /**
         * Fetches a SyncOperation's related BatchItems
         * 
         * @param SyncOperation operation 
         * @returns Promise: resolved with related BatchItem list, rejected with Database Error 
         */
        this.getBatchItems = function(operation) {
            return swdbDAO.findByQuery("Batch", "syncoperation = '{0}'".format(operation.id))
                .then(function(batches) {
                    var batchids = batches.map(function(batch) {
                        return "'{0}'".format(batch.id);
                    });
                    return swdbDAO.findByQuery("BatchItem", "batch in ({0})".format(batchids));
                });
        };
    };

    modules.webcommons.service("synchronizationOperationService", ["$log", "$q", "swdbDAO", "formatService", SynchronizationOperationService]);

})();