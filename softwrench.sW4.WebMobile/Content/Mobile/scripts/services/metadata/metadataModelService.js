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
            /// <summary>
            /// Receives a list of metadas from the server, and update the memory instance and the database, 
            /// so that next time the customer has the updated data.
            /// 
            /// 
            /// </summary>
            /// <param name="serverMetadatas"></param>
            /// <returns type=""></returns>
            var defer = $q.defer();
            if (isArrayNullOrEmpty(serverMetadatas)) {
                defer.resolve();
                return defer.promise;
            }

            var instancesToSavePromises = [];
            var instancesToDelete = [];

            for (var i = 0; i < serverMetadatas.length; i++) {
                var applicationMetadata = serverMetadatas[i];
                instancesToSavePromises.push(loadEntityInstance(applicationMetadata));
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

            return $q.all(instancesToSavePromises)
                .then(function (results) {
                return $q.all([swdbDAO.bulkSave(results), swdbDAO.bulkDelete(instancesToDelete)]);
            }).then(function (savedInstances) {
                    //updating the model, only if save is actually performed
                    metadataModel.applications = savedInstances[0];
                    defer.resolve();
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