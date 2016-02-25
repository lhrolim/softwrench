(function (softwrench) {
    "use strict";

    softwrench.controller('CrudCompositionDetailController', ["$log", "$scope", "$rootScope", "crudContextService", "fieldService", "offlineCompositionService", "offlineAssociationService", "schemaService",
    function ($log, $scope, $rootScope, crudContextService, fieldService, offlineCompositionService, offlineAssociationService, schemaService) {

        function init() {
            $scope.schema = crudContextService.getCompositionDetailSchema();
            $scope.displayables =  schemaService.allDisplayables($scope.schema);
            $scope.datamap = crudContextService.getCompositionDetailItem();
            $scope.allowsUpdate = offlineCompositionService.allowsUpdate(crudContextService.getCompositionDetailItem(), crudContextService.getCompositionListSchema());
        }

        $scope.fieldLabel = function (item, field) {
            return field.label + ":" + item[field.attribute];
        }

        $scope.associationSearch = function (query, componentId) {
            return offlineAssociationService.filterPromise($scope.schema, $scope.datamap, componentId, query);
        }

        $scope.getAssociationLabelField = function (fieldMetadata) {
            return offlineAssociationService.fieldLabelExpression(fieldMetadata);
        }

        $scope.getAssociationValueField = function (fieldMetadata) {
            return offlineAssociationService.fieldValueExpression(fieldMetadata);
        }

        $scope.visibleFields = function () {
            var schema = crudContextService.compositionListSchema();
            return fieldService.getVisibleDisplayables({}, schema);
        }
        $scope.isFieldHidden = function (fieldMetadata) {
            return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
        }

        $rootScope.$on('$stateChangeSuccess',
        function (event, toState, toParams, fromState, fromParams) {
            if (toState.name.startsWith("main.cruddetail.compositiondetail")) {
                //needs to refresh the displayables and datamap everytime the detail page is loaded.
                init();
            }
        });

        init();

    }]);
})(softwrench);



