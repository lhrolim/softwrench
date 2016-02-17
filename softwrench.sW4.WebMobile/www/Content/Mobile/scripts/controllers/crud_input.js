(function (angular) {
    "use strict";

    angular.module("softwrench").controller("CrudInputController", ['$log', '$scope', 'crudContextService', 'fieldService', 'offlineAssociationService', '$ionicPopover', 'expressionService', function ($log, $scope, crudContextService, fieldService, offlineAssociationService, $ionicPopover, expressionService) {

        $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
            scope: $scope,
        }).then(function (popover) {
            $scope.compositionpopover = popover;
        });


        //$scope.$on("sw_validationerrors", function (event, validationArray) {

        //});


        $scope.associationSearch = function(query, componentId) {
            return offlineAssociationService.filterPromise(crudContextService.currentDetailSchema(), $scope.datamap, componentId, query);
        };

        function testEmpty(label) {
            return "(!!" + label + " && " + label + " !== \'null\' && " + label + " !== \'undefined\')";
        }

        $scope.getAssociationLabelField = function (fieldMetadata) {
            var associationValueField = this.getAssociationValueField(fieldMetadata);
            if ("true" === fieldMetadata.hideDescription) {
                return associationValueField;
            }

            var label = "datamap." + fieldMetadata.labelFields[0];

            return "(" + testEmpty(associationValueField) +  " ? " + associationValueField + " : \'\' ) + " + 
                    "(" + testEmpty(label) + " ? (\' - \'  + "  + label + ") : \'\')";
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
    
})(angular);


