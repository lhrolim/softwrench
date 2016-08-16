﻿(function (mobileServices) {
    "use strict";

    mobileServices.factory('associationDataSynchronizationService', ["$http", "$log", "$q", "swdbDAO", "metadataModelService", "offlineRestService", "rowstampService",
        "offlineEntities", "searchIndexService", function ($http, $log, $q, swdbDAO, metadataModelService, restService, rowstampService, offlineEntities, searchIndexService) {
    return {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationToFetch">a single application to fetch. If not provided, all the applications would be fetched</param>
        /// <returns type=""></returns>
        syncData: function (applicationToFetch) {
            var log = $log.get("associationDataSynchronizationService#syncData");

            const currentApps = metadataModelService.getApplicationNames();
            const firstTime = currentApps.length === 0;

            var methodToUse = applicationToFetch ? "PullSingleAssociationData" : "PullAssociationData";
            var params = {};
            if (applicationToFetch) {
                params.applicationToFetch = applicationToFetch;
            }
            return rowstampService.generateAssociationRowstampMap(applicationToFetch).then(function (rowstampMap) {
                return restService.post("Mobile", methodToUse, params, rowstampMap);
            }).then(function (result) {
                const associationData = result.data.associationData;
                if (result.data.isEmpty) {
                    log.info("no new data returned from the server");
                    //interrupting async calls
                    return $q.reject();
                }
                var queryArray = [];
                //for first time, let´s not use the replace keyword in order to make the query faster (we know for sure they are all insertions)
                //TODO: check possibility of having different arrays
                const queryToUse = firstTime ? offlineEntities.AssociationData.InsertionPattern.format("") : offlineEntities.AssociationData.InsertionPattern.format(" or REPLACE ");

                for (let app in associationData) {
                    const dataToInsert = associationData[app];
                    const textIndexes = result.data.textIndexes[app];
                    const numericIndexes = result.data.numericIndexes[app];
                    const dateIndexes = result.data.dateIndexes[app];
                    for (let j = 0; j < dataToInsert.length; j++) {
                        const datamap = dataToInsert[j];
                        const id = persistence.createUUID();
                        const idx = searchIndexService.buildIndexes(textIndexes, numericIndexes, dateIndexes, datamap);
                        const query = { query: queryToUse, args: [datamap.application, JSON.stringify(datamap), String(datamap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };
                        queryArray.push(query);
                    }
                }
                return swdbDAO.executeQueries(queryArray).then(function () {
                    //returning the number of associationData created
                    return $q.when(queryArray.length);
                });
            }).catch(function (err) {
                if (!err) {
                    //normal interruption
                    return $q.when(0);
                }
                return $q.reject(err);
            });


        }
    }
}]);

})(mobileServices);