mobileServices.factory('metadataModelService', function ($q, swdbDAO, dispatcherService) {

    var metadataModel = {
        applications: [],
    };

    function loadEntityInstance(serverMetadata) {

        var applicationName = serverMetadata.applicationName;

        var applications = metadataModel.applications;
        var loadedEntity = $.findFirst(applications, function (el) {
            return el.application == applicationName;
        });
        var dbId = null;
        if (loadedEntity) {
            //setup database id for merging
            dbId = loadedEntity.id;
        }
        return swdbDAO.instantiate('Application', { id: dbId }, function (memoryObject, entity) {
            entity.data = serverMetadata;
            entity.application = applicationName;
            return entity;
        });
    }


    return {

        getMetadatas: function () {
            return metadataModel.schemas;
        },

        updateMetadata: function (serverMetadatas) {
            var defer = $q.defer();
            if (!serverMetadatas) {
                defer.resolve();
                return defer.promise;
            }

            var instancesToSavePromises = [];
            var instancesToDelete = [];

            for (var i = 0; i < serverMetadatas.length; i++) {
                //server return a list of CompleteApplicationMetadata, but we're saving each schema individually
                var applicationMetadata = serverMetadatas[i];
                instancesToSavePromises.push(loadEntityInstance(applicationMetadata));
                //var schemaList = applicationMetadata.schemaList;
                //for (var j = 0; j < schemaList.length; j++) {

                //}
            }

            for (var j = 0; j < metadataModel.applications.length; j++) {
                var entity = metadataModel.applications[j];
                var serverInstance = $.findFirst(serverMetadatas, function (el) {
                    return el.applicationName == entity.application;
                });
                if (!serverInstance) {
                    //this means that this entry was not returned from the server, and hence needs to be deleted
                    instancesToDelete.push(entity);
                }
            }
            return swdbDAO.bulkDelete(instancesToDelete).then(function () {
                $q.all(instancesToSavePromises)
                    .then(function (instancesToSave) {
                        swdbDAO.bulkSave(instancesToSave).success(function () {
                            //updating the model, only if save is actually performed
                            metadataModel.applications = instancesToSave;
                            defer.resolve();
                        });
                    });
            });
        },

        initAndCacheFromDB: function () {
            var defer = $q.defer();
            swdbDAO.findAll("Application").success(function (applications) {
                metadataModel.applications = applications;
            });
            return defer.promise;

        },

    }
});