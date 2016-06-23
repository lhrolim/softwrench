(function (mobileServices) {
    "use strict";

    mobileServices.factory('offlineSaveService', ["$log", "swdbDAO", "$ionicPopup", "offlineEntities", "offlineAttachmentService", function ($log, swdbDAO, $ionicPopup, offlineEntities, offlineAttachmentService) {

        var entities = offlineEntities;

        var doSave = function (applicationName, item, messageEntry, showConfirmationMessage) {
            const localId = item.id;
            item.isDirty = true;
            var queryToExecute;
            item.datamap["#offlinesavedate"] = new Date();
            const jsonString = JSON.stringify(item.datamap);
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
                const datamap = item.datamap;
                const associationKey = compositionMetadata.associationKey;
                datamap[associationKey] = datamap[associationKey] || [];
                compositionItem[constants.localIdKey] = persistence.createUUID();
                if (compositionMetadata.associationKey === "attachment_") {
                    return offlineAttachmentService.saveAttachment(null, compositionItem).then((attachmentObj) => {
                        compositionItem["#offlinehash"] = attachmentObj.id;
                        //this will be stored on the Attachement entity instead
                        delete compositionItem["newattachment"];
                        datamap[associationKey].push(compositionItem);
                        return doSave(applicationName, item, compositionMetadata.label);
                    });
                }

                datamap[associationKey].push(compositionItem);
                return doSave(applicationName, item, compositionMetadata.label);
            },
        }

    }]);

})(mobileServices);