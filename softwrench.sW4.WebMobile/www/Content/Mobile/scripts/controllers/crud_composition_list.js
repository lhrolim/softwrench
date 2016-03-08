(function (softwrench) {
    "use strict";

    softwrench.controller('CrudCompositionListController', ["$log", "$scope", "$rootScope", "crudContextService", "fieldService", "formatService",
    function ($log, $scope, $rootScope, crudContextService, fieldService, formatService) {

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

        $scope.visibleFields = function () {
            var schema = crudContextService.getCompositionListSchema();
            return fieldService.getVisibleDisplayables({}, schema);
        }

        $scope.loadCompositionDetail = function (item) {
            crudContextService.loadCompositionDetail(item);
        }

        $scope.isDirty = function (item) {
            return item[constants.localIdKey];
        }

    }]);

})(softwrench);



