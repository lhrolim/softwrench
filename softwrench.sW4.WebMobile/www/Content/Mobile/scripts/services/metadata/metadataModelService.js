(function (mobileServices) {
    "use strict";

    mobileServices.factory('metadataModelService', ["$q", "$log", "swdbDAO", "dispatcherService", function ($q, $log, swdbDAO, dispatcherService) {

    var initialMetadataModel = {
        topLevelApplications: [],
        compositionApplications: [],
        associationApplications: []
    }
    
    //so that we have the original object available for resetting it
    var metadataModel = angular.copy(initialMetadataModel);

    function loadEntityInstance(serverMetadata, memoryArrayName, mergefunctionCBK) {

        var applicationName = serverMetadata.applicationName;
        const applications = metadataModel[memoryArrayName];
        const loadedEntity = $.findFirst(applications, function (el) {
            return el.application === applicationName;
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


    function doUpdateMetadata(serverMetadatas, memoryArrayName, mergefunctionCbk) {
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
        const memoryArray = metadataModel[memoryArrayName];
        if (isArrayNullOrEmpty(serverMetadatas)) {
            defer.resolve();
            return defer.promise;
        }
        const instancesToSavePromises = [];
        var instancesToDelete = [];

        for (let i = 0; i < serverMetadatas.length; i++) {
            const applicationMetadata = serverMetadatas[i];
            instancesToSavePromises.push(loadEntityInstance(applicationMetadata, memoryArrayName, mergefunctionCbk));
        }

        for (let j = 0; j < memoryArray.length; j++) {
            var entity = memoryArray[j];
            const serverInstance = $.findFirst(serverMetadatas, function (el) {
                return el.applicationName === entity.application;
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
                metadataModel[memoryArrayName] = savedInstances[0];
                defer.resolve();
            });
    };


    return {

        getMetadatas: function () {
            return metadataModel.topLevelApplications;
        },

        getApplicationByName: function (applicationName, includeAssociations) {
            const metadatas = this.getMetadatas();
            for (let i = 0; i < metadatas.length; i++) {
                const metadata = metadatas[i];
                if (metadata.application === applicationName) {
                    return metadata;
                }
            }
            if (!!includeAssociations) {
                return metadataModel.associationApplications.find(a => a.application === applicationName);
            }
            return null;
        },

     

        getApplicationNames: function () {
            const appNames = [];
            const metadatas = this.getMetadatas();
            if (!metadatas) {
                return appNames;
            }
            for (let i = 0; i < metadatas.length; i++) {
                appNames.push(metadatas[i].application);
            }
            return appNames;
        },


        updateTopLevelMetadata: function (serverMetadatas) {
            return doUpdateMetadata(serverMetadatas, 'topLevelApplications',
                function (memoryObject, entity) {
                    entity.association = false;
                    entity.composition = false;
                    return entity;
                });
        },

        updateCompositionMetadata: function (serverMetadatas) {
            return doUpdateMetadata(serverMetadatas, 'compositionApplications',
                function (memoryObject, entity) {
                    entity.association = false;
                    entity.composition = true;
                    return entity;
                });
        },

        updateAssociationMetadata: function (serverMetadatas) {
            return doUpdateMetadata(serverMetadatas, 'associationApplications',
                function (memoryObject, entity) {
                    entity.association = true;
                    entity.composition = false;
                    return entity;
                });
        },


        initAndCacheFromDB: function () {
            var log = $log.getInstance("metadataModelService#initAndCacheFromDB",["init","metadata", "botstrap"]);
            var defer = $q.defer();
            swdbDAO.findAll("Application").then(function (applications) {
                metadataModel.associationApplications = [];
                metadataModel.compositionApplications = [];
                metadataModel.topLevelApplications = [];
                for (let i = 0; i < applications.length; i++) {
                    const application = applications[i];
                    if (application.association) {
                        log.info("caching association {0}".format(application.application));
                        metadataModel.associationApplications.push(application);
                    } else if (application.composition) {
                        log.info("caching composition {0}".format(application.application));
                        metadataModel.compositionApplications.push(application);
                    } else {
                        log.info("caching topLevel App {0}".format(application.application));
                        metadataModel.topLevelApplications.push(application);
                    }
                }
                defer.resolve();
            });
            return defer.promise;

        },

        reset:function() {
            metadataModel = angular.copy(initialMetadataModel);
        }

    }
}]);

})(mobileServices);
