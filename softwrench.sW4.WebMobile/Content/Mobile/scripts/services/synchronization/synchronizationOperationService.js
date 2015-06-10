modules.webcommons.factory("synchronizationOperationService", function ($q,swdbDAO) {

    'use strict';

    var internalListContext = {
        lastPageLoaded: 1
    }

    var context = {
        list: null
    }

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
        },

        refreshSync: function () {
            context.list = null;
        },

        hasProblems: function () {
            //TODO:implement
            return false;
        },

        getSyncList: function () {
            if (context.list) {
                return context.list;
            }
            context.list = [];
            return swdbDAO.findByQuery('SyncOperation', "", { pagesize: 10, pagenumber: internalListContext.pageNumber, orderby:'startdate', orderbyascending:false }).then(function(results) {
                context.list = results;
                return $q.when();
            });
        }

    }

})