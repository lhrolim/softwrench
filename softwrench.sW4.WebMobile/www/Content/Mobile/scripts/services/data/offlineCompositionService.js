(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineCompositionService', ["$log", "swdbDAO", "offlineEntities", function ($log, swdbDAO, offlineEntities) {

    var entities = offlineEntities;

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
                var idsToDelete = [];
                var tempArray = [];
                for (var j = 0; j < newDataMaps.length; j++) {
                    var datamap = newDataMaps[j];
                    var id = persistence.createUUID();
                    var query = {query: entities.CompositionDataEntry.insertionQueryPattern, args: [datamap.application, JSON.stringify(datamap.fields), datamap.id, String(datamap.approwstamp), id]};
                    idsToDelete.push("'" + datamap.id + "'");
                    queryArray.push(query);
                }
                //let´s delete the old compositions first, to avoid any chance of errors due to server side updates
                //however persistence.js reverts the array... damn it
                queryArray.push({ query: entities.CompositionDataEntry.syncdeletionQuery, args: [idsToDelete] });
            }
            return queryArray;
        },

        allowsUpdate: function (item, compositionListSchema) {
            //only items not yet synced with the server
            //TODO: someday allow synced compositions as well
            return item[compositionListSchema.idFieldName] == null;
        },


        loadComposition: function (mainItem, displayable) {
            var mainDatamap = mainItem.datamap;
            if (!displayable) {
                throw new Error("field displayable is required");
            }
            //TODO: cache...
            var log = $log.get("offlineCompositionService#loadComposition");
            var localId = mainItem.id;
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
            baseQuery += " or ( parentlocalId = '{0}') )".format(localId);
            log.debug("fetching composition {0} using query {1}".format(displayable.associationKey, baseQuery));
            return swdbDAO.findByQuery("CompositionDataEntry", baseQuery, { projectionFields: ["remoteId", "datamap"] }).then(function (results) {
                var resultCompositions = [];
                for (i=0; i< results.length;i++) {
                    resultCompositions.push(results[i].datamap);
                }
                // put any locally created compositions on top of the list
                if (mainDatamap[displayable.associationKey]) {
                    resultCompositions = mainDatamap[displayable.associationKey].concat(resultCompositions);
                }
                return resultCompositions;
            });

        }


    }

}]);

})(mobileServices);