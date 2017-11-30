(function (mobileServices, angular, _) {
    "use strict";

    //private functions
    let userDataIfChanged, invokeCustomServicePromise, buildIdsString, errorHandlePromise, entities;

    class dataSynchronizationService {



        constructor($http, $q, $log, swdbDAO, dispatcherService, offlineRestService, metadataModelService, rowstampService, offlineCompositionService, offlineEntities, searchIndexService, securityService, applicationStateService, configurationService, settingsService) {
            this.$http = $http;
            this.$q = $q;
            this.$log = $log;
            this.swdbDAO = swdbDAO;
            this.dispatcherService = dispatcherService;
            this.restService = offlineRestService;
            this.metadataModelService = metadataModelService;
            this.rowstampService = rowstampService;
            this.offlineCompositionService = offlineCompositionService;

            this.searchIndexService = searchIndexService;
            this.securityService = securityService;
            this.applicationStateService = applicationStateService;
            this.configurationService = configurationService;
            this.settingsService = settingsService;

            entities = offlineEntities;


            userDataIfChanged = function () {
                const current = securityService.currentFullUser();
                if (!current) {
                    return securityService.logout();
                }
                return current.meta && current.meta.changed ? current : null;
            };

            invokeCustomServicePromise = (result, queryArray) => {
                return $q.when(dispatcherService.invokeService(`${result.data.clientName}.dataSynchronizationHook`, 'modifyQueries', [result.data, queryArray])).then(() => queryArray);
            }


            buildIdsString = function (deletedRecordIds) {
                var ids = [];
                angular.forEach(deletedRecordIds, function (id) {
                    ids.push("'{0}'".format(id));
                });
                return ids;
            };

            errorHandlePromise = function (error) {
                if (!error) {
                    return $q.when();
                }
                return $q.reject(error);
            };




        }

        /**
         * 
         * @param {*} firstInLoop 
         * @param {*} app null if this is a full sync, or a specific application for a quick sync
         * @param {*} currentApps the list of all available applications
         * @param {*} compositionMap a map of all compositions rowstamps, to
         * @param {*} clientOperationId for auditing the operation at server side
         */
        createAppSyncPromise(firstInLoop, app, currentApps, compositionMap, clientOperationId) {
            var log = this.$log.get("dataSynchronizationService#createAppSyncPromise");

            const resultHandlePromise = this.resultHandlePromise.bind(this);
            const that = this;

            return this.applicationStateService.getServerDeviceData()
                .then(deviceData => {
                    return that.rowstampService.generateRowstampMap(app)
                        .then(rowstampMap => {
                            return { deviceData, rowstampMap }
                        });
                }).then(({ rowstampMap, deviceData }) => {
                    //see samplerequest.json
                    rowstampMap.compositionmap = compositionMap;
                    log.debug("invoking service to get new data");
                    const payload = {
                        applicationName: app,
                        clientCurrentTopLevelApps: currentApps,
                        returnNewApps: firstInLoop,
                        clientOperationId,
                        userData: userDataIfChanged(),
                        rowstampMap,
                        deviceData
                    };
                    return that.restService.post("Mobile", "PullNewData", null, payload);
                })
                .then(resultHandlePromise);
        }



        /**
         *  Returns an object (promise) containing the query array to run and the number of downloads (which excludes the compositions count)
         * 
         * 
         * @param {*} result coming from MobileController#PullNewData 
         */
        generateQueriesPromise(result) {
            const data = result.data;
            const log = this.$log.get("dataSynchronizationService#generateQueries", ["sync"]);

            const topApplicationData = data.topApplicationData;
            const compositionData = data.compositionData;

            const userProperties = data.userProperties;
            const fullUser = this.securityService.currentFullUser();

            const currentFacilities = (fullUser && fullUser.properties && fullUser.properties["sync.facilities"]) || [];
            const serverFacilities = (userProperties && userProperties["sync.facilities"]) || [];
            const facilityChanges = data.facilitiesUpdated || !_.isEqual(currentFacilities.sort(), serverFacilities.sort());
            this.configurationService.getFullConfig(ConfigurationKeys.FacilitiesChanged).then(config => {
                const save = config === null || (config && config.value === false && facilityChanges);
                if (save) {
                    this.configurationService.saveConfig({ key: ConfigurationKeys.FacilitiesChanged, value: facilityChanges });
                }
            });

            this.securityService.overrideCurrentUserProperties(userProperties);

            //do not modify to const as this array is modified internally to append compositions and custom entries
            let queryArray = [];

            if (data.isEmpty) {
                log.info("no new data returned from the server");
                return invokeCustomServicePromise(result, queryArray).then(queryArray => {
                    //interrupting async calls
                    const numberOfDownloadedItems= 0;
                    return { queryArray, numberOfDownloadedItems };
                })

            }

            log.info("receiving new topLevel data from the server");

            angular.forEach(topApplicationData, application => {
                //multiple applications can be returned on a limit scenario where it´s the first sync, or on a server update.
                const newDataMaps = application.newdataMaps;
                const updatedDataMaps = application.updatedDataMaps;
                const insertUpdateDatamap = application.insertOrUpdateDataMaps;
                const deletedIds = application.deletedRecordIds;
                log.debug("{0} topleveldata: inserting:{1} | updating:{2} | deleting: {3}".format(application.applicationName, newDataMaps.length, updatedDataMaps.length, deletedIds.length));

                angular.forEach(newDataMaps, newDataMap => {
                    const id = persistence.createUUID();

                    const newJson = newDataMap.jsonFields || JSON.stringify(newDataMap); //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);
                    const datamap = newDataMap.jsonFields ? JSON.parse(newDataMap.jsonFields) : newDataMap; //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);

                    const idx = this.searchIndexService.buildIndexes(application.textIndexes, application.numericIndexes, application.dateIndexes, datamap);
                    const insertQuery = { query: entities.DataEntry.insertOrReplacePattern, args: [newDataMap.application, newJson, newDataMap.id, String(newDataMap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };
                    queryArray.push(insertQuery);
                });

                angular.forEach(insertUpdateDatamap, insertOrUpdateDatamap => {
                    const id = persistence.createUUID();

                    const newJson = insertOrUpdateDatamap.jsonFields || JSON.stringify(insertOrUpdateDatamap); //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);
                    const datamap = insertOrUpdateDatamap.jsonFields ? JSON.parse(insertOrUpdateDatamap.jsonFields) : insertOrUpdateDatamap; //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);

                    const idx = this.searchIndexService.buildIndexes(application.textIndexes, application.numericIndexes, application.dateIndexes, datamap);
                    const insertOrUpdateQuery = { query: entities.DataEntry.insertOrReplacePattern, args: [insertOrUpdateDatamap.application, newJson, insertOrUpdateDatamap.id, String(insertOrUpdateDatamap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };
                    queryArray.push(insertOrUpdateQuery);
                });

                angular.forEach(updatedDataMaps, updateDataMap => {
                    const updateJson = updateDataMap.jsonFields || JSON.stringify(updateDataMap); // keeping backward compatibility //updateJson = datamapSanitizationService.sanitize(updateJson);
                    const datamap = updateDataMap.jsonFields ? JSON.parse(updateDataMap.jsonFields) : updateDataMap; //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);

                    const idx = this.searchIndexService.buildIndexes(application.textIndexes, application.numericIndexes, application.dateIndexes, datamap);
                    const updateQuery = { query: entities.DataEntry.updateQueryPattern, args: [updateJson, String(updateDataMap.approwstamp), idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3, updateDataMap.id, updateDataMap.application] };
                    queryArray.push(updateQuery);
                });

                if (deletedIds.length > 0) {
                    const fnName = isRippleEmulator() ? "push" : "unshift";
                    const deleteQuery = { query: entities.DataEntry.deleteQueryPattern.format(buildIdsString(deletedIds), application.applicationName) };
                    queryArray[fnName](deleteQuery);
                    //TODO: treat the case where AuditEntries that have no refId shouldn't be deleted (e.g. crud_create operations)
                    const deleteAuditQuery = {
                        query: entities.AuditEntry.deleteRelatedByRefIdStatement.format(buildIdsString(deletedIds)),
                        args: [application.apllicationName]
                    };
                    queryArray[fnName](deleteAuditQuery);
                }
            });

            //test
            // ignoring composition number to SyncOperation table
            const numberOfDownloadedItems = queryArray.length;

            return this.offlineCompositionService.generateSyncQueryArrays(compositionData)
                .then(compositionQueriesToAppend => queryArray.concat(compositionQueriesToAppend))
                .then((queryArray) => {
                    return invokeCustomServicePromise(result, queryArray);
                }).then(queryArray => {
                    return { queryArray, numberOfDownloadedItems };
                })

        }


        resultHandlePromise(result) {

            return this.generateQueriesPromise(result).then(result => {
                return this.swdbDAO.executeQueries(result.queryArray).then(() => {
                    return result.numberOfDownloadedItems;
                })
            });
        }

        /**
         *  Main sync operation, receives a clientOperationId for storing an audit entry at server side.
         * 
         *  
         * 
         * @param {String} clientOperationId for storing an offline audit entry at server side
         */
        syncData(clientOperationId) {


            const resultHandlePromise = this.resultHandlePromise.bind(this);
            const createAppSyncPromise = this.createAppSyncPromise.bind(this);
            const that = this;

            return this.applicationStateService.getServerDeviceData()
                .then(deviceData => {
                    var currentApps = this.metadataModelService.getApplicationNames();
                    const firstTime = currentApps.length === 0;
                    var payload;
                    if (firstTime) {
                        //upon first synchronization let's just bring them all, since we don´t even know what are the metadatas
                        payload = {
                            clientCurrentTopLevelApps: currentApps,
                            returnNewApps: true,
                            clientOperationId,
                            deviceData,
                            userData: userDataIfChanged()
                        };
                        //single server call
                        return that.restService.post("Mobile", "PullNewData", null, payload)
                            .then(resultHandlePromise)
                            .catch(errorHandlePromise);
                    }
                    return that.rowstampService.generateCompositionRowstampMap()
                        .then(function (compositionMap) {
                            const httpPromises = [];
                            const promise = createAppSyncPromise(true, null, currentApps, compositionMap, clientOperationId).catch(errorHandlePromise);
                            httpPromises.push(promise);

                            return that.$q.all(httpPromises);
                        });
                });


        }

        syncSingleItem(item, clientOperationId) {
            const app = item.application;
            const resultHandlePromise = this.resultHandlePromise.bind(this);

            return this.applicationStateService.getServerDeviceData().then(deviceData => {
                return this.rowstampService.generateCompositionRowstampMap().then(compositionMap => {
                    const rowstampMap = {
                        compositionmap: compositionMap
                    }
                    const payload = {
                        applicationName: app,
                        itemsToDownload: [item.remoteId],
                        userData: userDataIfChanged(),
                        rowstampMap,
                        deviceData,
                        clientOperationId
                    };
                    var promise = this.restService.post("Mobile", "PullNewData", null, payload).then(resultHandlePromise)
                        .catch(errorHandlePromise);
                    return this.$q.all([promise]);
                });
            });
        }

    }

    dataSynchronizationService.$inject = ["$http", "$q", "$log", "swdbDAO", "dispatcherService", "offlineRestService", "metadataModelService", "rowstampService", "offlineCompositionService", "offlineEntities", "searchIndexService", "securityService", "applicationStateService", "configurationService", "settingsService"];

    mobileServices.service('dataSynchronizationService', dataSynchronizationService);

})(mobileServices, angular, _);
