mobileServices.factory('offlineCompositionService', function ($log) {

    return {

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
        }


    }

});