softwrench.controller('CrudCompositionListController',
    function ($log, $scope, $rootScope, crudContextService, fieldService) {

        'use strict';


        $scope.empty = function () {
            var compositionList = crudContextService.compositionList();
            return !compositionList || compositionList.length == 0;
        }

        $scope.list = function () {
            return crudContextService.compositionList();
        }

        $scope.fieldLabel = function (item,field) {
            return field.label + ":" + item[field.attribute];
        }

        $scope.visibleFields = function () {
            var schema = crudContextService.getCompositionListSchema();
            return fieldService.getVisibleDisplayables({}, schema);
        }

        $scope.loadCompositionDetail = function(item) {
            crudContextService.loadCompositionDetail(item);
        }

       



    }
);