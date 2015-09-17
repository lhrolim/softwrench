(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineSaveService', ["$log", "swdbDAO", "$ionicPopup", "offlineEntities", function ($log, swdbDAO, $ionicPopup, offlineEntities) {

        var entities = offlineEntities;

        var doSave = function (applicationName, item, messageEntry, showConfirmationMessage) {
            var localId = item.id;
            item.isDirty = true;
            var queryToExecute;
            item.datamap["#offlinesavedate"] = new Date();
            var jsonString = JSON.stringify(item.datamap);
            if (!localId) {
                queryToExecute = { query: entities.DataEntry.insertLocalPattern, args: [applicationName, jsonString, persistence.createUUID()] };
            } else {
                queryToExecute = { query: entities.DataEntry.updateLocalPattern, args: [jsonString, localId] };
            }
            return swdbDAO.executeQuery(queryToExecute).then(function () {
                if (showConfirmationMessage === undefined || showConfirmationMessage === true) {
                    return $ionicPopup.alert({
                        title: "{0} saved successfully".format(messageEntry),
                    });
                }
                return item;
            });
        };

        return {

            saveItem: function (applicationName, item, showConfirmationMessage) {
                return doSave(applicationName, item, applicationName, showConfirmationMessage);
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