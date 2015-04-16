mobileServices.factory('metadataModelService', function ($q,swdbDAO,dispatcherService) {

    var metadataModel = {
        schemas: {},
    };


    return {

        getMetadatas:function() {
            return metadataModel.schemas;
        },

        updateMetadata: function (serverMetadatas) {
            var defer = $q.defer();
            metadataModel.schemas = serverMetadatas;
            defer.resolve();
            return defer.promise;
        },

        initAndCacheFromDB: function () {
            var defer =$q.defer();
            swdbDAO.findAll("Schema").success(function (schemas) {
                metadataModel.schemas = schemas;
            });
            return defer.promise;

        },

    }
});