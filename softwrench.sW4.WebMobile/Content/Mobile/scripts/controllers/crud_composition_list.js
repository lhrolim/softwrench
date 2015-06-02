softwrench.controller('CrudCompositionListController',
    function ($log, $scope, $rootScope, crudContextService, fieldService) {

        'use strict';


        $scope.empty = function () {
            return crudContextService.compositionList().length==0;
        }

        $scope.list = function () {
            return crudContextService.compositionList();
        }

        $scope.fieldLabel = function (item,field) {
            return field.label + ":" + item[field.attribute];
        }

        $scope.visibleFields = function () {
            var schema = crudContextService.compositionListSchema();
            return fieldService.getVisibleDisplayables({}, schema);
        }



    }
);