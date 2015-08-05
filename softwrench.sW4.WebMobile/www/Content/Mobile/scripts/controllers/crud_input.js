
(function () {
    "use strict";

    angular.module("softwrench").controller("CrudInputController", ['$log', '$scope', 'crudContextService', 'fieldService', 'offlineAssociationService', '$ionicPopover', 'expressionService', function ($log, $scope, crudContextService, fieldService, offlineAssociationService, $ionicPopover, expressionService) {

        $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
            scope: $scope,
        }).then(function (popover) {
            $scope.compositionpopover = popover;
        });


        $scope.$on("sw_validationerrors", function (event, validationArray) {

        });

     

        $scope.associationSearch = function (query, componentId) {
            return offlineAssociationService.filterPromise(crudContextService.currentDetailSchema(), $scope.datamap, componentId, query);
        }

        $scope.getAssociationLabelField = function (fieldMetadata) {
            return 'datamap.' + fieldMetadata.labelFields[0];
        }

        $scope.getAssociationValueField = function (fieldMetadata) {
            return 'datamap.' + fieldMetadata.valueField;
        }

        $scope.isFieldHidden = function (fieldMetadata) {
            return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
        }

        $scope.isFieldRequired = function (requiredExpression) {
            if (requiredExpression != undefined) {
                return expressionService.evaluate(requiredExpression, $scope.datamap);
            }
            return requiredExpression;
        };

    }]);

})();


