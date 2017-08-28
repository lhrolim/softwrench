(function(angular, modules) {
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
    const SynchronizationOperationService = function ($log, $q, swdbDAO, formatter) {

        const saveBatchOperation = function (operation, relatedBatches) {
            return swdbDAO.instantiate("SyncOperation", operation)
                .then((operationEntity) => {
                    // all batches need to be complete in order for the syncoperation to be complete
                    const isComplete = relatedBatches.every(batch => batch.status === "COMPLETE");
                    // add relationships
                    relatedBatches.forEach(batch => {
                        operationEntity.batches.add(batch);
                        if (batch.loadeditems && batch.loadeditems !== null && batch.loadeditems.length > 0) {
                            operationEntity.items += batch.loadeditems.length;
                        }
                        batch.syncoperation = operationEntity;
                    });
                    if (isComplete) {
                        //if every batch returned as complete than we have a synchronous case and can close the sync operation
                        const hasProblem = relatedBatches.some(result => result.hasProblems);
                        operationEntity.status = "COMPLETE";
                        operationEntity.enddate = new Date().getTime();
                        operationEntity.hasProblems = hasProblem;
                    } else {
                        operationEntity.status = "PENDING";
                    }
                    var deferred = $q.defer();
                    persistence.transaction(tx => {
                        try {
                            swdbDAO.bulkSave(relatedBatches, tx);
                            swdbDAO.save(operationEntity, tx);
                            // flushing transaction
                            persistence.flush(tx, () => {
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
            const operation = {
                startdate: startdate,
                items: 0
            };
            // return promise chain initiated with the creation of SyncOperation
            return saveBatchOperation(operation, relatedBatches);
        };

        this.createSynchronousBatchOperation = function (startdate,clientOperationId, numberofdownloadeditems, relatedBatches) {
            //if every batch returned as complete than we have a synchronous case and can close the sync operation
            const hasProblem = relatedBatches.some(result => result.hasProblems);
            if (!clientOperationId) {
                clientOperationId = persistence.createUUID();
            }


            const operation = {
                startdate,
                numberofdownloadeditems,
                numberofdownloadedsupportdata: 0,
                hasProblems: hasProblem,
                clientOperationId,
                enddate: new Date()
            };
            return saveBatchOperation(operation, relatedBatches);
        };
        
        this.createNonBatchOperation = function (startdate, clientOperationId, enddate, numberofdownloadeditems, numberofdownloadedsupportdata, metadatachange) {
            const operation = {
                clientOperationId,
                startdate,
                enddate,
                status: "COMPLETE",
                numberofdownloadeditems,
                numberofdownloadedsupportdata,
                metadatachange
            };
            return swdbDAO.instantiate("SyncOperation", operation).then(item => swdbDAO.save(item));
        }

        this.hasProblems = function (operation) {
            return operation.hasProblems;
        };

        this.getSyncList = function (pageNumber) {
            const page = !pageNumber || pageNumber <= 0 ? 1 : pageNumber;
            return swdbDAO.findByQuery("SyncOperation", null, { pagesize: 10, pageNumber: page, orderby: "startdate", orderbyascending: false })
                .then(operations => 
                    !operations || operations.length <= 0 
                        ? operations
                        // format the operations found
                        : operations.map(operation => this.formatOperation(operation))
                );
        };

        /**
         * Fetches the SyncOperation the has the matching id 
         * 
         * @param int id primary key
         * @returns Promise: resolved with SyncOperation entity, rejected with database Error. 
         */
        this.getOperation = function (id) {
            return swdbDAO.findById("SyncOperation", id).then(operation => this.formatOperation(operation));
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
                .then(batches => {
                    const batchids = batches.map(batch => `'${batch.id}'`);
                    return swdbDAO.findByQuery("BatchItem", "batch in ({0})".format(batchids), { prefetch: "problem" });
                });
        };

        /**
         * Fetches the most recent (highest 'startdate') SyncOperation from the database.
         * 
         * @returns Promise: resolved with most recent operation, rejected with database error 
         */
        this.getMostRecentOperation = function() {
            return swdbDAO.findSingleByQuery("SyncOperation", null, { orderby: "startdate", orderbyascending: false })
                .then(operation => this.formatOperation(operation));
        };

        /**
         * Updates the status of the SyncOperations related to the batches to "COMPLETE"
         * and sets their enddate as now.
         * 
         * TODO: this method assumes there's a single batch/application per SyncOperation -> develop generic case
         * 
         * @param [Batch] batches 
         * @returns Promise resolved with array of updated SyncOperations 
         */
        this.completeFromAsyncBatch = function(batches) {
            var enddate = new Date();
            const operations = batches.map(function(batch) {
                batch.syncoperation.status = "COMPLETE";
                batch.syncoperation.enddate = enddate;
                return batch.syncoperation;
            });
            return swdbDAO.bulkSave(operations);
        };

    };

    modules.webcommons.service("synchronizationOperationService", ["$log", "$q", "swdbDAO", "formatService", SynchronizationOperationService]);

})(angular, modules);