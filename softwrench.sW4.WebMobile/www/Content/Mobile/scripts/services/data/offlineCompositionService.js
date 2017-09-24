(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineCompositionService', ["$log", "swdbDAO", "offlineEntities", "attachmentDataSynchronizationService", "searchIndexService",
        function ($log, swdbDAO, offlineEntities, attachmentDataSynchronizationService, searchIndexService) {

            var entities = offlineEntities;



            /**
             * Generates the queries to update the composition entries after a synchronization has been performed.
             * @param {any} compositionDataReturned The composition data returned from the server, a dictionary of multiple applications (worklogs,attachments, etc)
             * @return type="string" a array of queries to be executed later
             */
            const generateSyncQueryArrays = function (compositionDataReturned) {
                const log = $log.get("compositionService#generateSyncQueryArrays", ["composition"]);
                let queryArray = [];

                if (compositionDataReturned == null) {
                    log.debug("no compositions returned from the server");
                    return queryArray;
                }

                const doclinksMap = new Map();

                for (let i = 0; i < compositionDataReturned.length; i++) {
                    const application = compositionDataReturned[i];
                    const newDataMaps = application.newdataMaps;

                    log.debug("inserting {0} new compositions for {1}".format(newDataMaps.length, application.applicationName));
                    const idsToDelete = [];
                    for (let j = 0; j < newDataMaps.length; j++) {
                        const datamap = newDataMaps[j];
                        const id = persistence.createUUID();
                        const json = datamap.jsonFields || JSON.stringify(datamap);
                        const parsedDM = datamap.jsonFields ? JSON.parse(datamap.jsonFields) : datamap; //keeping backwards compatibility //newJson = datamapSanitizationService.sanitize(newJson);

                        const idx = searchIndexService.buildIndexes(application.textIndexes, application.numericIndexes, application.dateIndexes, JSON.parse(datamap.jsonFields));
                        const query = { query: entities.CompositionDataEntry.insertionQueryPattern, args: [datamap.application, json, datamap.id, String(datamap.approwstamp), id, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] };

                        idsToDelete.push("'" + datamap.id + "'");
                        queryArray.push(query);
                        if (application.applicationName === "attachment_") {
                            var queryObj = { compositionRemoteId: parsedDM.doclinksid, hash: parsedDM["docinfo_.urlparam1"], ownerTable: parsedDM.ownertable, ownerId: parsedDM.ownerid, docinfoid: parsedDM.docinfoid };
                            doclinksMap.set(queryObj.compositionRemoteId, queryObj);
                        }
                    }
                    if (idsToDelete.length !== 0) {
                        //let´s delete the old compositions first, to avoid any chance of errors due to server side updates
                        //however persistence.js reverts the array on ripple... damn it
                        if (isRippleEmulator()) {
                            queryArray.push(entities.CompositionDataEntry.syncdeletionQuery.format(application.applicationName, idsToDelete));
                        } else {
                            queryArray.unshift(entities.CompositionDataEntry.syncdeletionQuery.format(application.applicationName, idsToDelete));
                        }


                    }

                }

                return attachmentDataSynchronizationService.generateAttachmentsQueryArray(Array.from(doclinksMap.values()))
                    .then((attachmentQueryArray) => {
                        queryArray = queryArray.concat(attachmentQueryArray);
                        log.debug(`final composition array count ${queryArray.length}`);
                        return queryArray;
                    });

            };

            const allowsUpdate = function (item, compositionListSchema) {
                //only items not yet synced with the server
                //TODO: someday allow synced compositions as well
                return item[compositionListSchema.idFieldName] == null;
            };
            /**
             * Load composition list
             * 
             * @param {} mainItem 
             * @param {} displayable 
             * @returns {} 
             */
            const loadCompositionList = function (mainItem, displayable) {
                var mainDatamap = mainItem.datamap;
                if (!displayable) {
                    throw new Error("field displayable is required");
                }
                //TODO: cache...
                const log = $log.get("offlineCompositionService#loadComposition", ["composition"]);
                const localId = mainItem.id;
                var baseQuery = "application = '{0}' and ( (".format(displayable.associationKey);
                const entityDeclarationAttributes = displayable.entityAssociation.attributes;
                for (var i = 0; i < entityDeclarationAttributes.length; i++) {
                    const attribute = entityDeclarationAttributes[i];
                    let fromValue;
                    if (attribute.literal) {
                        //siteid = 'SOMETHING'
                        fromValue = attribute.literal;
                    } else {
                        //siteid = siteid
                        fromValue = mainDatamap[attribute.from];
                    }
                    baseQuery += '( datamap like \'%"{0}":"{1}"%\' '.format(attribute.to, fromValue);
                    baseQuery += ' or datamap like \'%"{0}":{1}%\' )'.format(attribute.to, fromValue);
                    if (i != entityDeclarationAttributes.length - 1) {
                        baseQuery += " and ";
                    }
                }
                baseQuery += "))";
                //baseQuery += " or ( parentlocalId = '{0}') )".format(localId);

                if (displayable.schema &&
                    displayable.schema.collectionProperties &&
                    displayable.schema.collectionProperties.orderByField &&
                    displayable.schema.schemas &&
                    displayable.schema.schemas.list) {

                    const listSchema = displayable.schema.schemas.list;
                    const appName = listSchema.applicationName;
                    const column = displayable.schema.collectionProperties.orderByField;
                    const orderIndex = searchIndexService.getIndexColumn(appName, listSchema, column);
                    baseQuery += ` order by ${orderIndex}`;
                }

                log.debug("fetching composition {0} using query {1}".format(displayable.associationKey, baseQuery));
                return swdbDAO.findByQuery("CompositionDataEntry", baseQuery, { projectionFields: ["remoteId", "datamap"] }).then(function (results) {
                    var resultCompositions = [];
                    for (let i = 0; i < results.length; i++) {
                        resultCompositions.push(results[i].datamap);
                    }
                    // put any locally created compositions on top of the list
                    if (mainDatamap[displayable.associationKey]) {
                        resultCompositions = mainDatamap[displayable.associationKey].concat(resultCompositions);
                    }
                    return resultCompositions;
                });

            };


            const api = {
                allowsUpdate,
                generateSyncQueryArrays,
                loadCompositionList,
            }

            return api;

        }]);

})(mobileServices);