﻿(function (softwrench, _) {
    "use strict";

    softwrench.controller('CrudCompositionListController', ["$log", "$scope", "$rootScope", "$ionicPopup", "crudContextService", "fieldService", "formatService", "crudContextHolderService", "offlineAttachmentService",
    function ($log, $scope, $rootScope, $ionicPopup, crudContextService, fieldService, formatService, crudContextHolderService, offlineAttachmentService) {

        $scope.empty = function () {
            var compositionList = crudContextService.compositionList();
            return !compositionList || compositionList.length === 0;
        }

        $scope.list = function () {
            return crudContextService.compositionList();
        }

        //$scope.fieldLabel = function (item, field) {
        //    return field.label + ":" + formatService.format(item[field.attribute], field, item);
        //}

        $scope.fieldValue = function(item, field) {
            return formatService.format(item[field.attribute], field, item);
        }

        $scope.visibleFields = function (item) {
            const schema = crudContextService.getCompositionListSchema();
            return fieldService.getVisibleDisplayables(item, schema);
        }

        $scope.loadCompositionDetail = function (item) {
            crudContextService.loadCompositionDetail(item);
        }

        // ReSharper disable once UnusedParameter
        const deleteCompositionFromLocalId = function (localId, datamap, relationship) {
            const compositions = datamap[relationship];
            if (!compositions) {
                return;
            }

            const index = _.findIndex(compositions, composition => composition["#localswdbid"] === localId);
            
            if (index >= 0) {
                compositions.splice(index, 1);
            }
        }

        $scope.startDeleteLocalComposition = function (item) {
            const localId = item["#localswdbid"];
            if (!localId) {
                return;
            }

            const compositionSchema = crudContextHolderService.getCompositionDetailSchema();
            const compositionTitle = compositionSchema["title"] || compositionSchema["applicationTitle"];
            console.log(crudContextService);

            $ionicPopup.confirm({
                title: `Delete ${compositionTitle}`,
                template: `Are you sure to delete this local ${compositionTitle}?`
            }).then(res => {
                if (!res) {
                    return;
                }

                const context = crudContextHolderService.getCrudContext();
                const tab = context.composition.currentTab;
                const relationship = tab["relationship"];

                const datamap = crudContextHolderService.currentDetailItemDataMap();
                deleteCompositionFromLocalId(localId, datamap, relationship);

                const originalDatamap = context.originalDetailItemDatamap;
                deleteCompositionFromLocalId(localId, originalDatamap, relationship);

                const compositionList = $scope.list();
                const index = compositionList.indexOf(item);
                compositionList.splice(index, 1);

                const savePromise = crudContextService.saveChanges(null, false);

                return compositionSchema.applicationName === "attachment" 
                    ? savePromise.then(saved => offlineAttachmentService.deleteRelatedAttachment(item).then(() => saved))
                    : savePromise;
            });
        }

        $scope.isDirty = function (item) {
            return item[constants.localIdKey];
        }

    }]);

})(softwrench, _);



