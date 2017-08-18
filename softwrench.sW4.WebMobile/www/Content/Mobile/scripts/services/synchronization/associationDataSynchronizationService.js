(function (mobileServices) {
    "use strict";

    mobileServices.factory("associationDataSynchronizationService",
        ["$http", "$log", "$q", "swdbDAO", "metadataModelService", "offlineRestService", "rowstampService", "offlineEntities", "searchIndexService", "securityService",
            function ($http, $log, $q, swdbDAO, metadataModelService, restService, rowstampService, offlineEntities, searchIndexService, securityService) {

                return {

                    doInsert: function (queryToUse, result, appArray) {

                        const queryArray = [];

                        for (let i = 0; i < appArray.length; i++) {
                            const app = appArray[i];

                            const associationData = result.data.associationData;
                            const textIndexes = result.data.textIndexes[app];
                            const numericIndexes = result.data.numericIndexes[app];
                            const dateIndexes = result.data.dateIndexes[app];

                            const associationDatadto = associationData[app];

                            let dataToInsert = associationDatadto.individualItems;
                            const remoteIdFieldName = associationDatadto.remoteIdFieldName;

                            for (let j = 0; j < dataToInsert.length; j++) {
                                const data = dataToInsert[j];
                                const id = persistence.createUUID();

                                const json = data.jsonFields || JSON.stringify(data);//keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);
                                const datamap = data.jsonFields ? JSON.parse(data.jsonFields) : data; //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);
                                const remoteid = datamap[remoteIdFieldName];

                                const idx = searchIndexService.buildIndexes(textIndexes, numericIndexes, dateIndexes, datamap);
                                const query = { query: queryToUse, args: [data.application, json, String(data.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3, remoteid] };
                                queryArray.push(query);
                            }
                        }
                        return swdbDAO.executeQueries(queryArray).then(() => {
                            return queryArray.length;
                        });
                    },


                    doInsertRecursively: function (queryToUse, result, apps, chunkSize, initialSliceIndex, count) {
                        const log = $log.get("associationDataSynchronizationService#doInsertRecursively", ["association", "sync"]);
                        var doInsertRecursivelyFn = this.doInsertRecursively.bind(this);
                        var finalIndex = initialSliceIndex + chunkSize;
                        if (finalIndex > apps.length) {
                            finalIndex = apps.length;
                        }
                        const appsToInsert = apps.slice(initialSliceIndex, finalIndex);
                        log.debug(`inserting association apps ${appsToInsert.join(',')}`);


                        return this.doInsert(queryToUse, result, appsToInsert).then(r => {
                            count += r;
                            if (apps.length > finalIndex) {
                                initialSliceIndex += chunkSize;
                                return doInsertRecursivelyFn(queryToUse, result, apps, chunkSize, initialSliceIndex, count);
                            }
                            return $q.when(count);
                        });
                    },

                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="applicationToFetch">a single application to fetch. If not provided, all the applications would be fetched</param>
                    /// <returns type=""></returns>
                    syncData: function (firstTime,applicationsToFetch =[], chunkRound =1, totalCount =0, completeCacheEntries =[]) {
                        const log = $log.get("associationDataSynchronizationService#syncData", ["association", "sync"]);

                        log.info("bringing server side data to apply based on rowstampmap");

                        const syncDataFn = this.syncData.bind(this);
                        
                        const doInsertFn = this.doInsertRecursively.bind(this);

                        const params = {};
                        
                        return rowstampService.generateAssociationRowstampMap(applicationsToFetch, firstTime).then(rowstampMap => {
                            const payload = { rowstampMap };
                            const current = securityService.currentFullUser();
                            if (current.meta && current.meta.changed) {
                                payload.userData = current;
                            }
                            payload.applicationsToFetch = applicationsToFetch;
                            payload.initialLoad = firstTime;
                            payload.completeCacheEntries = completeCacheEntries;
                            return restService.post("Mobile", "PullAssociationData", params, payload);
                        }).then(result => {
                            const associationData = result.data.associationData;

                            if (result.data.isEmpty) {
                                return totalCount;
                            }

                            //for first time, let´s not use the replace keyword in order to make the query faster (we know for sure they are all insertions)
                            //TODO: check possibility of having different arrays
                            const queryToUse = firstTime ? offlineEntities.AssociationData.InsertionPattern.format("") : offlineEntities.AssociationData.InsertionPattern.format(" or REPLACE ");

                            const apps = Object.keys(associationData);
                            return doInsertFn(queryToUse, result, apps, apps.length, 0, 0).then(r => {
                                totalCount += r;
                                if (result.data.hasMoreData) {
                                    var appsforLog = result.data.incompleteAssociations.join(",");
                                    log.info(`bringing next round of chunked data (${++chunkRound}) for applications ${appsforLog}`);
                                    return syncDataFn(firstTime,result.data.incompleteAssociations, chunkRound, totalCount, result.data.completeCacheEntries);
                                }
                                return totalCount;
                            });
                        }).catch(err =>
                            !err
                                ? $q.when(0) // normal interruption 
                                : $q.reject(err)
                            );


                    }
                }
            }]);

})(mobileServices);