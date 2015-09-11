(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineSaveService', ["$log", "swdbDAO", "$ionicPopup", "offlineEntities", function ($log, swdbDAO, $ionicPopup, offlineEntities) {

    var entities = offlineEntities;

    var doSave = function (applicationName, item, messageEntry) {
        var localId = item.id;
        item.isDirty = true;
        var queryToExecute;
        var jsonString = JSON.stringify(item.datamap);
        if (!localId) {
            queryToExecute = { query: entities.DataEntry.insertLocalPattern, args: [applicationName, jsonString, persistence.createUUID()] };
        } else {
            queryToExecute = { query: entities.DataEntry.updateLocalPattern, args: [jsonString, localId] };
        }
        return swdbDAO.executeQuery(queryToExecute).then(function () {
            return $ionicPopup.alert({
                title: "{0} saved successfully".format(messageEntry),
            });
        });
    };

    return {

        saveItem: function (applicationName, item) {
            return doSave(applicationName, item, applicationName);
        },

        addAndSaveComposition: function (applicationName, item, compositionItem, compositionMetadata) {
            var datamap = item.datamap;
            var associationKey = compositionMetadata.associationKey;
            datamap[associationKey] = datamap[associationKey] || [];
            compositionItem[constants.localIdKey] = persistence.createUUID();
            datamap[associationKey].push(compositionItem);
            return doSave(applicationName, item, compositionMetadata.label);
        },
    }

}]);

})(mobileServices);