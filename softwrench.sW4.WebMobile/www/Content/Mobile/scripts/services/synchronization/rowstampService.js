(function (mobileServices) {
    "use strict";

    mobileServices.constant("rowstampConstants", {
        //above this limit framework shall no longer produce the full rowstamp map, but rather just pass the maxrowstamp to the server
        maxItemsForFullStrategy: 1000
    });

    mobileServices.factory('rowstampService', ["$q", "$log", "swdbDAO", "offlineEntities", "rowstampConstants", function ($q, $log, swdbDAO, offlineEntities, rowstampConstants) {
        var entities = offlineEntities;

        return {

            generateRowstampMap: function (application) {
                var log = $log.get("rowstampService#generateRowstampMap");
                var start = new Date().getTime();
                
                var applicationQuery = !application  ? "1=1" : "application ='{0}'".format(application);
                //https://controltechnologysolutions.atlassian.net/browse/SWOFF-140
                var shouldUseFullStrategy = true;
                return swdbDAO.countByQuery("DataEntry", applicationQuery)
                    .then(function (result) {

                        shouldUseFullStrategy = result < rowstampConstants.maxItemsForFullStrategy;
                        //for small datasets we sall bring it all
                        if (!shouldUseFullStrategy) {
                            if (!application) {
                                return swdbDAO.findByQuery('DataEntry', null, { fullquery: entities.DataEntry.maxRowstampGeneralQuery });
                            }

                            return swdbDAO.findByQuery('DataEntry', null, { fullquery: entities.DataEntry.maxRowstampByAppQuery.format(application) });
                        }
                        return swdbDAO.findByQuery("DataEntry", applicationQuery, { projectionFields: ["application","remoteId", "rowstamp"] });
                    }).then(function (queryResults) {
                        const rowstampMap = {};
                        if (shouldUseFullStrategy) {
                            //small dataset scenario
                            if (application != null) {
                                const resultItems = queryResults.map(function(item) {
                                    return {
                                        id: item.remoteId,
                                        rowstamp: item.rowstamp
                                    }
                                });
                                rowstampMap.items = resultItems;
                                const end = new Date().getTime();
                                log.debug("generated rowstampmap for application {0} with {1} entries. Ellapsed {2} ms"
                                    .format(application, resultItems.length, (end - start)));
                                return rowstampMap;
                            } else {
                                const applications = {};
                                for (let i = 0; i < queryResults.length; i++) {
                                    var item = queryResults[i];
                                    if (!applications[item.application]) {
                                        applications[item.application] = {};
                                    }
                                    if (!applications[item.application].items) {
                                        applications[item.application].items = [];
                                    }
                                    applications[item.application].items.push({
                                        id: item.remoteId,
                                        rowstamp: item.rowstamp
                                    });
                                }
                                rowstampMap.applications = applications;
                                return rowstampMap;
                            }
                        } else {
                            if (application != null) {
                                rowstampMap.maxrowstamp = queryResults[0].rowstamp;
                                rowstampMap.application = application;
                            } else {
                                debugger;
                                rowstampMap.applications = queryResults.map(m => {return {[m.application]: m.rowstamp} });
                            }

                            
                            return rowstampMap;
                        }
                    });
            },

            generateCompositionRowstampMap: function () {
                var log = $log.get("rowstampService#generateCompositionRowstampMap");
                var deferred = $q.defer();
                var start = new Date().getTime();
                swdbDAO.findByQuery('CompositionDataEntry', null, { fullquery: entities.CompositionDataEntry.maxRowstampQueries })
                    .then(function (queryResults) {
                        const resultItems = {};
                        for (let i = 0; i < queryResults.length; i++) {
                            const item = queryResults[i];
                            resultItems[item.application] = item.rowstamp;
                        }
                        const end = new Date().getTime();
                        log.debug("generated rowstampmap for compositions. Ellapsed {0} ms".format(end - start));
                        deferred.resolve(resultItems);
                    });
                return deferred.promise;
            },

            generateAssociationRowstampMap: function (apps, initialLoad) {
                var log = $log.get("rowstampService#generateAssociationRowstampMap", ["sync","association"]);
                var deferred = $q.defer();
                var start = new Date().getTime();
                //either for query for a single app, or for all of them
                //TODO: use AssociationCache in the future
                var formattedApps = null;
                const hasApps = !!apps && apps.length > 0;

                if (hasApps) {
                    formattedApps = "'" + apps.join("','") + "'";
                }
                let query;
                if (initialLoad) {
                    query = hasApps ? entities.AssociationData.maxRemoteIdQueryByApps.format(formattedApps) : entities.AssociationData.maxRemoteIdQueries;    
                } else {
                    query = hasApps ? entities.AssociationData.maxRowstampQueryByApps.format(formattedApps) : entities.AssociationData.maxRowstampQueries;    
                }

                
                swdbDAO.findByQuery('AssociationData', null, { fullquery: query })
                    .then(function (queryResults) {
                        const result = {};
                        const associationmap = {};
                        for (let i = 0; i < queryResults.length; i++) {
                            const item = queryResults[i];
                            if (initialLoad) {
                                associationmap[item.application] = { "maximouid": item.remoteid }
                            } else {
                                associationmap[item.application] = { "maximorowstamp": item.rowstamp }
                            }
                            
                        }
                        const end = new Date().getTime();
                        log.debug("generated rowstampmap for associations. Ellapsed {0} ms".format(end - start));
                        result.associationmap = associationmap;
                        deferred.resolve(result);
                    });
                return deferred.promise;
            }



        }
    }]);

})(mobileServices);