modules.webcommons.factory("synchronizationOperationService", function (swdbDAO) {

    'use strict';

    return {

        createBatchOperation: function (startdate, relatedBatches) {
            var operation = {
                startdate: startdate,
                status: 'PENDING',
                batches: relatedBatches
            };
            return swdbDAO.instantiate('SyncOperation', operation).then(function (item) {
                return swdbDAO.save(item);
            });
        },

        createNonBatchOperation: function (startdate, enddate, numberofdownloadeditems, numberofdownloadedsupportdata, metadatachange) {
            var operation = {
                startdate: startdate,
                enddate: enddate,
                status: 'COMPLETE',
                numberofdownloadeditems: numberofdownloadeditems,
                numberofdownloadedsupportdata: numberofdownloadedsupportdata,
                metadatachange: metadatachange,
            };
            return swdbDAO.instantiate('SyncOperation', operation).then(function (item) {
                return swdbDAO.save(item);
            });
        }

    }

})