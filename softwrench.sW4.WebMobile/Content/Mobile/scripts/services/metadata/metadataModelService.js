mobileServices.factory('metadataModelService', function ($q, swdbDAO, dispatcherService) {

    var metadataModel = {
        topLevelApplications: [],
        compositionApplications: [],
        associationApplications: [],
    };

    function loadEntityInstance(serverMetadata, mergefunctionCBK) {

        var applicationName = serverMetadata.applicationName;

        var applications = metadataModel.topLevelApplications;
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
            mergefunctionCBK(memoryObject, entity);
            return entity;
        });
    }


    function doUpdateMetadata(serverMetadatas, memoryArray, mergefunctionCbk) {
        /// <summary>
        /// Receives a list of metadas from the server, and update the memory instance and the database, 
        /// so that next time the customer has the updated data.
        /// 
        /// 
        /// </summary>
        /// <param name="serverMetadatas">an array of metadatas received from the server</param>
        /// <param name="memoryArray">which memory array to use</param>
        /// <returns type="promise"></returns>
        var defer = $q.defer();
        if (isArrayNullOrEmpty(serverMetadatas)) {
            defer.resolve();
            return defer.promise;
        }

        var instancesToSavePromises = [];
        var instancesToDelete = [];

        for (var i = 0; i < serverMetadatas.length; i++) {
            var applicationMetadata = serverMetadatas[i];
            instancesToSavePromises.push(loadEntityInstance(applicationMetadata, mergefunctionCbk));
        }

        for (var j = 0; j < memoryArray.length; j++) {
            var entity = memoryArray[j];
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
                memoryArray = savedInstances[0];
                defer.resolve();
            });
    };


    return {

        getMetadatas: function () {
            return metadataModel.topLevelApplications;
        },

        getApplicationNames: function () {
            var appNames = [];
            var metadatas = this.getMetadatas();
            if (!metadatas) {
                return appNames;
            }
            for (var i = 0; i < metadatas.length; i++) {
                appNames.push(metadatas[i].application);
            }
            return appNames;
        },

        updateTopLevelMetadata: function (serverMetadatas) {
            return this.doUpdateMetadata(serverMetadatas, metadataModel.associationApplications,
                function (memoryObject, entity) {
                    entity.association = false;
                    entity.composition = false;
                    return entity;
                });
        },

        updateCompositionMetadata: function (serverMetadatas) {
            return this.doUpdateMetadata(serverMetadatas, metadataModel.associationApplications,
                function (memoryObject, entity) {
                    entity.association = false;
                    entity.composition = true;
                    return entity;
                });
        },

        updateAssociationMetadata: function (serverMetadatas) {
            return this.doUpdateMetadata(serverMetadatas, metadataModel.topLevelApplications,
                function (memoryObject, entity) {
                    entity.association = true;
                    entity.composition = false;
                    return entity;
                });
        },



        initAndCacheFromDB: function () {
            var defer = $q.defer();
            swdbDAO.findAll("Application").success(function (applications) {
                metadataModel.topLevelApplications = applications;
            });
            return defer.promise;

        },

    }
});