(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineCompositionService', ["$log", "swdbDAO", "offlineEntities", "attachmentDataSynchronizationService",
        function ($log, swdbDAO, offlineEntities, attachmentDataSynchronizationService) {

            var entities = offlineEntities;



            /// <summary>
            ///  Generates the queries to update the composition entries after a synchronization has been performed.
            /// </summary>
            /// <param name="compositionDataReturned">The composition data returned from the server, a dictionary of multiple applications (worklogs,attachments, etc)</param>
            /// <returns type="CompositionSyncResult"></returns>
            const generateSyncQueryArrays = function (compositionDataReturned) {
                const log = $log.get("compositionService#generateSyncQueryArrays");
                let queryArray = [];

                if (compositionDataReturned == null) {
                    log.debug("no compositions returned from the server");
                    return queryArray;
                }

                const doclinksArray = [];

                for (let i = 0; i < compositionDataReturned.length; i++) {
                    const application = compositionDataReturned[i];
                    const newDataMaps = application.newdataMaps;

                    log.debug("inserting {0} new compositions for {1}".format(newDataMaps.length, application.applicationName));
                    const idsToDelete = [];
                    const tempArray = [];
                    for (let j = 0; j < newDataMaps.length; j++) {
                        const datamap = newDataMaps[j];
                        const id = persistence.createUUID();
                        const fields = datamap.fields;

                        const query = { query: entities.CompositionDataEntry.insertionQueryPattern, args: [datamap.application, JSON.stringify(fields), datamap.id, String(datamap.approwstamp), id] };

                        idsToDelete.push("'" + datamap.id + "'");
                        queryArray.push(query);
                        if (application.applicationName === "attachment_") {
                            doclinksArray.push({ compositionRemoteId: fields.doclinksid, hash: fields["docinfo_.urlparam5"], ownerTable: fields.ownertable, ownerId: fields.ownerid, docinfoid: fields.docinfoid });
                        }
                    }
                    //let´s delete the old compositions first, to avoid any chance of errors due to server side updates
                    //however persistence.js reverts the array... damn it
                    queryArray.push({ query: entities.CompositionDataEntry.syncdeletionQuery, args: [idsToDelete] });
                }

                return attachmentDataSynchronizationService.generateAttachmentsQueryArray(doclinksArray)
                    .then((attachmentQueryArray) => queryArray.concat(attachmentQueryArray));

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
                const log = $log.get("offlineCompositionService#loadComposition");
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