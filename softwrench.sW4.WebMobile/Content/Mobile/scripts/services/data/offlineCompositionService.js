mobileServices.factory('offlineCompositionService', function ($log, swdbDAO) {

    'use strict';

    return {

        /// <summary>
        ///  Generates the queries to update the composition entries after a synchronization has been performed.
        /// </summary>
        /// <param name="compositionDataReturned">The composition data returned from the server, a dictionary of multiple applications (worklogs,attachments, etc)</param>
        /// <returns type="Array">An array of queries to be executed for updating the compositionEntries</returns>
        generateSyncQueryArrays: function (compositionDataReturned) {

            var log = $log.get("compositionService#generateSyncQueryArrays");

            var queryArray = [];
            if (compositionDataReturned == null) {
                log.debug("no compositions returned from the server");
                return queryArray;
            }

            for (var i = 0; i < compositionDataReturned.length; i++) {
                var application = compositionDataReturned[i];
                var newDataMaps = application.newdataMaps;
                log.debug("inserting {0} new compositions for {1}".format(newDataMaps.length, application.applicationName));
                var idsToDelete = "";
                var tempArray = [];
                for (var j = 0; j < newDataMaps.length; j++) {
                    var datamap = newDataMaps[j];
                    var id = persistence.createUUID();
                    var query = entities.CompositionDataEntry.insertionQueryPattern.format(datamap.application, JSON.stringify(datamap.fields), datamap.id, '' + datamap.approwstamp, id);
                    idsToDelete += ("'" + datamap.id + "'");
                    if (j != newDataMaps.length - 1) {
                        idsToDelete += ",";
                    }
                    queryArray.push(query);
                }
                //let´s delete the old compositions first, to avoid any chance of errors due to server side updates
                //however persistence.js reverts the array... damn it
                queryArray.push(entities.CompositionDataEntry.syncdeletionQuery.format(idsToDelete));
            }
            return queryArray;
        },

        allowsUpdate: function (item, compositionListSchema) {
            //only items not yet synced with the server
            //TODO: someday allow synced compositions as well
            return item[compositionListSchema.idFieldName] == null;
        },


        loadComposition: function (mainDatamap, displayable) {
            if (!displayable) {
                throw new Error("field displayable is required");
            }
            //TODO: cache...
            var log = $log.get("offlineCompositionService#loadComposition");
            var localId = mainDatamap[constants.localIdKey];
            var baseQuery = "application = '{0}' and ( (".format(displayable.associationKey);
            var entityDeclarationAttributes = displayable.entityAssociation.attributes;

            for (var i = 0; i < entityDeclarationAttributes.length; i++) {
                var attribute = entityDeclarationAttributes[i];
                var fromValue;
                if (attribute.literal) {
                    //siteid = 'SOMETHING'
                    fromValue = attribute.literal;
                } else {
                    //siteid = siteid
                    fromValue = mainDatamap[attribute.from];
                }
                baseQuery += ' datamap like \'%"{0}":"{1}"%\' '.format(attribute.to, fromValue);
                if (i != entityDeclarationAttributes.length - 1) {
                    baseQuery += " and ";
                }
            }
            baseQuery += ")";
            baseQuery += " or ( parentId = '{0}') )".format(localId);
            log.debug("fetching composition {0} using query {1}".format(displayable.associationKey, baseQuery));
            return swdbDAO.findByQuery("CompositionDataEntry", baseQuery, { projectionFields: ["remoteId", "datamap"] }).then(function (results) {
                var resultCompositions = [];
                mainDatamap[displayable.associationKey] = mainDatamap[displayable.associationKey] || [];
                for (i=0; i< results.length;i++) {
                    resultCompositions.push(results[i].datamap);
                }
                // put any locally created compositions on top of the list
                resultCompositions = resultCompositions.concat(mainDatamap[displayable.associationKey]);
                return resultCompositions;
            });

        }


    }

});