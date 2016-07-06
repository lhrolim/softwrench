(function (mobileServices) {
    "use strict";

    mobileServices.factory('associationDataSynchronizationService', ["$http", "$log", "$q", "swdbDAO", "offlineRestService", "rowstampService", "offlineEntities", "searchIndexService", function ($http, $log, $q, swdbDAO, restService, rowstampService, offlineEntities, searchIndexService) {
    return {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationToFetch">a single application to fetch. If not provided, all the applications would be fetched</param>
        /// <returns type=""></returns>
        syncData: function (applicationToFetch) {
            var log = $log.get("associationDataSynchronizationService#syncData");

            var methodToUse = applicationToFetch ? "PullSingleAssociationData" : "PullAssociationData";
            var params = {};
            if (applicationToFetch) {
                params.applicationToFetch = applicationToFetch;
            }
            return rowstampService.generateAssociationRowstampMap(applicationToFetch).then(function (rowstampMap) {
                return restService.post("Mobile", methodToUse, params, rowstampMap);
            }).then(function (result) {
                var associationData = result.data.associationData;
                if (result.data.isEmpty) {
                    log.info("no new data returned from the server");
                    //interrupting async calls
                    return $q.reject();
                }
                var queryArray = [];
                for (var app in associationData) {
                    var dataToInsert = associationData[app];
                    const textIndexes = result.data.textIndexes[app];
                    const dateIndexes = result.data.dateIndexes[app];
                    for (var j = 0; j < dataToInsert.length; j++) {
                        var datamap = dataToInsert[j];
                        var id = persistence.createUUID();
                        const idx = searchIndexService.buildIndexes(textIndexes, dateIndexes, datamap);
                        var query = { query: offlineEntities.AssociationData.InsertionPattern, args: [datamap.application, JSON.stringify(datamap.fields), datamap.id, String(datamap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.d1, idx.d2, idx.d3] };
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