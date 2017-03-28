(function (mobileServices) {
    "use strict";

    mobileServices.factory("associationDataSynchronizationService",
        ["$http", "$log", "$q", "swdbDAO", "metadataModelService", "offlineRestService", "rowstampService", "offlineEntities", "searchIndexService", "securityService",
    function ($http, $log, $q, swdbDAO, metadataModelService, restService, rowstampService, offlineEntities, searchIndexService, securityService) {

        return {

            doInsert: function (queryToUse, result, app) {

                const associationData = result.data.associationData;
                const dataToInsert = associationData[app];
                const textIndexes = result.data.textIndexes[app];
                const numericIndexes = result.data.numericIndexes[app];
                const dateIndexes = result.data.dateIndexes[app];

                let queryArray = [];
                for (let j = 0; j < dataToInsert.length; j++) {
                    const datamap = dataToInsert[j];
                    const id = persistence.createUUID();
                    const idx = searchIndexService.buildIndexes(textIndexes, numericIndexes, dateIndexes, datamap);
                    const query = { query: queryToUse, args: [datamap.application, JSON.stringify(datamap), String(datamap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };
                    queryArray.push(query);
                }
                return swdbDAO.executeQueries(queryArray).then(() => {
                    return queryArray.length;
                });
            },


            doInsertRecursively: function (queryToUse, result, apps, i, count) {
                var doInsertRecursivelyFn = this.doInsertRecursively.bind(this);
                return this.doInsert(queryToUse, result, apps[i]).then(r => {
                    count += r;
                    if (i < apps.length-1) {
                        i++;
                        return doInsertRecursivelyFn(queryToUse, result, apps, i,count);
                    }
                    return $q.when(count);
                });
            },

            /// <summary>
            /// 
            /// </summary>
            /// <param name="applicationToFetch">a single application to fetch. If not provided, all the applications would be fetched</param>
            /// <returns type=""></returns>
            syncData: function (applicationToFetch) {
                const log = $log.get("associationDataSynchronizationService#syncData", ["sync"]);

                const currentApps = metadataModelService.getApplicationNames();
                const firstTime = currentApps.length === 0;
                const doInsertFn = this.doInsertRecursively.bind(this);

                const methodToUse = applicationToFetch ? "PullSingleAssociationData" : "PullAssociationData";
                const params = {};
                if (applicationToFetch) {
                    params.applicationToFetch = applicationToFetch;
                }

                return rowstampService.generateAssociationRowstampMap(applicationToFetch).then(rowstampMap => {
                    const payload = { rowstampMap };
                    const current = securityService.currentFullUser();
                    if (current.meta && current.meta.changed) {
                        payload.userData = current;
                    }
                    return restService.post("Mobile", methodToUse, params, payload);
                })
                .then(result => {
                    const associationData = result.data.associationData;
                    if (result.data.isEmpty) {
                        log.info("no new data returned from the server");
                        //interrupting async calls
                        return $q.reject();
                    }

                    //for first time, let´s not use the replace keyword in order to make the query faster (we know for sure they are all insertions)
                    //TODO: check possibility of having different arrays
                    const queryToUse = firstTime ? offlineEntities.AssociationData.InsertionPattern.format("") : offlineEntities.AssociationData.InsertionPattern.format(" or REPLACE ");

                    const apps = Object.keys(associationData);
                    return doInsertFn(queryToUse, result, apps, 0,0).then(r => {
                        return r;
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