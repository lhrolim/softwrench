mobileServices.factory('offlineSaveService', function ($log, swdbDAO, $ionicPopup) {

    'use strict';


    var doSave = function (applicationName, item, messageEntry) {
        var localId = item.id;
        item.isDirty = true;
        var queryToExecute = "";
        var jsonString = JSON.stringify(item.datamap);
        if (!localId) {
            queryToExecute = entities.DataEntry.insertLocalPattern.format(applicationName, jsonString, persistence.createUUID());
        } else {
            queryToExecute = entities.DataEntry.updateLocalPattern.format(jsonString, localId);
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

        addAndSaveComposition: function (applicationName, datamap, compositionItem, compositionMetadata) {
            var associationKey = compositionMetadata.associationKey;
            datamap[associationKey] = datamap[associationKey] || [];
            compositionItem[constants.localIdKey] = persistence.createUUID();
            datamap[associationKey].push(compositionItem);
            return doSave(applicationName, datamap, compositionMetadata.label);
        },
    }

});