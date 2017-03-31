(function (mobileServices, _) {
    "use strict";

    mobileServices.factory('offlineSaveService', ["$log", "$rootScope", "swdbDAO", "$ionicPopup", "offlineEntities", "offlineAttachmentService", "crudContextHolderService", "searchIndexService","metadataModelService", function ($log, $rootScope, swdbDAO, $ionicPopup, offlineEntities, offlineAttachmentService, crudContextHolderService, searchIndexService,metadataModelService) {



        var entities = offlineEntities;

        const doSave = function (applicationName, item, title, showConfirmationMessage) {
            const idxArrays = crudContextHolderService.getIndexes();
            const idx = searchIndexService.buildIndexes(idxArrays.textIndexes, idxArrays.numericIndexes, idxArrays.dateIndexes, item.datamap);

            const jsonString = JSON.stringify(item.datamap);
            const localId = item.id;
            const generatedId = localId ? null : persistence.createUUID();

            const isAlreadyDirty = _.contains([true, 1, "true"], item.isDirty);
            const cameFromServer = !!item.remoteId;
            const shouldIncludeOriginalDatamap = !isAlreadyDirty && cameFromServer; // not created locally and first time saving
            const originalDatamap = crudContextHolderService.getCrudContext().originalDetailItemDatamap;

            item.isDirty = true;
            item.datamap["#offlinesavedate"] = new Date();

            const queryToExecute = !localId
                // inserting new item
                ? { query: entities.DataEntry.insertLocalPattern, args: [applicationName, jsonString, generatedId, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3] }
                // updating existing item
                : shouldIncludeOriginalDatamap 
                    // setting originaldatamap
                    ? { query: entities.DataEntry.updateLocalSetOriginalPattern, args: [jsonString, JSON.stringify(item.originaldatamap = originalDatamap), idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3, localId] }
                    // regular update
                    : { query: entities.DataEntry.updateLocalPattern, args: [jsonString, idx.t1, idx.t2, idx.t3, idx.t4, idx.t5, idx.n1, idx.n2, idx.d1, idx.d2, idx.d3, localId] };

            const log = $log.get("offlineSaveService#doSave", ["crud", "detail", "save"]);
            log.debug(`saving item ${localId} for application ${applicationName}`);

            return swdbDAO.executeQuery(queryToExecute)
                .then(() => {
                    if (!localId) {
                        item.newId = generatedId;
                    }
                    $rootScope.$broadcast(JavascriptEventConstants.CrudSaved);
                    return showConfirmationMessage === undefined || showConfirmationMessage === null || showConfirmationMessage === true
                        ? $ionicPopup.alert({ title: `${title} Saved Successfully` }).then(() => item)
                        : item;
                });
        };

        return {

            saveItem: function (applicationName, item, title, showConfirmationMessage) {
                return doSave(applicationName, item, title, showConfirmationMessage);
            },

            addAndSaveComposition: function (applicationName, item, compositionItem, compositionMetadata) {
                const datamap = item.datamap;
                const associationKey = compositionMetadata.associationKey;
                const compositionAppMetadata = metadataModelService.getCompositionByName(compositionMetadata.attribute);
                const title = (compositionAppMetadata && compositionAppMetadata.data && compositionAppMetadata.data.title) || compositionMetadata.label;
                datamap[associationKey] = datamap[associationKey] || [];

                const log = $log.get("offlineSaveService#addAndSaveComposition", ["crud", "detail", "save"]);
                log.debug(`adding and saving a new composition ${compositionMetadata.associationKey} for application ${applicationName}`);

                const isLocalCreate = !compositionItem[constants.localIdKey];
                if (isLocalCreate) {
                    compositionItem[constants.localIdKey] = persistence.createUUID();
                }

                if (compositionMetadata.associationKey === "attachment_") {
                    return offlineAttachmentService.saveAttachment(null, compositionItem).then( attachmentObj => {
                        compositionItem["#offlinehash"] = attachmentObj.id;
                        //this will be stored on the Attachement entity instead
                        delete compositionItem["newattachment"];
                        datamap[associationKey].push(compositionItem);
                        return doSave(applicationName, item, title);
                    });
                }

                if (isLocalCreate) {
                    datamap[associationKey].push(compositionItem);
                } else {
                    // TODO: when we allow update of remote compositions, change this to delete the corresponding CompositionDataEntry and then pushing to the array
                    const itemPosition = datamap[associationKey].findIndex(e => e[constants.localIdKey] === compositionItem[constants.localIdKey]);
                    datamap[associationKey][itemPosition] = compositionItem;
                }
                return doSave(applicationName, item, title);
            },
        }

    }]);

})(mobileServices, _);
