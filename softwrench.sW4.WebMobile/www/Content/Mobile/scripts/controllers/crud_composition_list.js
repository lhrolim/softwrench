(function (softwrench, _) {
    "use strict";

    softwrench.controller('CrudCompositionListController', ["$log", "$scope", "$rootScope", "$ionicPopup", "crudContextService", "fieldService", "formatService", "crudContextHolderService", "offlineAttachmentService", "$injector",
    function ($log, $scope, $rootScope, $ionicPopup, crudContextService, fieldService, formatService, crudContextHolderService, offlineAttachmentService, $injector) {

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

        function handlerOrDefault(event, defaultAction, ...params) {
            if (!event || !$injector.has(event.service)) {
                return defaultAction();
            }
            const predeleteHandler = $injector.getInstance(event.service);
            if (!angular.isFunction(predeleteHandler[event.method])) {
                return defaultAction();
            }
            return predeleteHandler[event.method](...params, defaultAction);
        }

        $scope.startDeleteLocalComposition = function (item) {
            const localId = item["#localswdbid"];
            if (!localId) {
                return;
            }

            const compositionSchema = crudContextHolderService.getCompositionDetailSchema();
            const compositionTitle = compositionSchema["title"] || compositionSchema["applicationTitle"];
            const compositionListSchema = crudContextHolderService.getCompositionListSchema();
            const context = crudContextHolderService.getCrudContext();
            const tab = context.composition.currentTab;
            const compositionMetadata = tab.schema;

            const preDeleteEvent = compositionMetadata.events["beforedelete"] || compositionListSchema.events["beforedelete"];
            const postDeleteEvent = compositionMetadata.events["afterdelete"] || compositionListSchema.events["afterdelete"];

            const defaultPreDeletePromise = () => $ionicPopup.confirm({
                title: `Delete ${compositionTitle}`,
                template: `Are you sure to delete this local ${compositionTitle}?`
            });
            const preDeletePromise = handlerOrDefault(preDeleteEvent, defaultPreDeletePromise, item);

            return preDeletePromise.then(res => {
                if (!res) {
                    return;
                }

                const relationship = tab["relationship"];

                const datamap = crudContextHolderService.currentDetailItemDataMap();
                deleteCompositionFromLocalId(localId, datamap, relationship);

                const originalDatamap = context.originalDetailItemDatamap;
                deleteCompositionFromLocalId(localId, originalDatamap, relationship);

                const compositionList = $scope.list();
                const index = compositionList.indexOf(item);
                compositionList.splice(index, 1);

                const savePromise = crudContextService.saveCurrentItem(false);

                const deletePromise = compositionSchema.applicationName === "attachment"
                    ? savePromise.then(saved => offlineAttachmentService.deleteRelatedAttachment(item).then(() => saved))
                    : savePromise;

                return deletePromise.then(saved => handlerOrDefault(postDeleteEvent, () => saved, item, saved));
            });


        }

        $scope.isDirty = function (item) {
            return item[constants.localIdKey];
        }

    }]);

})(softwrench, _);



