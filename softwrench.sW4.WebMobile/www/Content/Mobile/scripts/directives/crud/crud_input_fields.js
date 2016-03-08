(function (softwrench) {
    "use strict";

softwrench.directive('sectionElementInput', ["$compile", function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            isDirty: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            extraparameters: '=',
            elementid: '@',
            orientation: '@',
            islabelless: '@',
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',

        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                "<crud-input-fields displayables='displayables'" +
                "schema='schema'" +
                "datamap='datamap'" +
                "is-dirty='isDirty'" +
                "displayables='displayables'" +
                "association-options='associationOptions'" +
                "association-schemas='associationSchemas'" +
                "blockedassociations='blockedassociations'" +
                "elementid='{{elementid}}'" +
                "orientation='{{orientation}}' insidelabellesssection='{{islabelless}}'" +
                "outerassociationcode='lookupAssociationsCode' outerassociationdescription='lookupAssociationsDescription' issection='true'" +
                "></crud-input-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
}]);

softwrench.directive('crudInputFields', [function () {

    return {
        restrict: 'E',
        replace: false,
        templateUrl: 'Content/Mobile/templates/directives/crud/crud_input_fields.html',
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
        },

        link: function (scope, element, attrs) {
            scope.name = "crud_input_fields";
        },

        controller: ["$scope", "offlineAssociationService", "fieldService", "expressionService", function ($scope, offlineAssociationService, fieldService, expressionService) {
            
            $scope.associationSearch = function (query, componentId) {
                return offlineAssociationService.filterPromise($scope.schema, $scope.datamap, componentId, query);
            };

            $scope.getAssociationLabelField = function (fieldMetadata) {
                return offlineAssociationService.fieldLabelExpression(fieldMetadata);
            }

            $scope.getAssociationValueField = function (fieldMetadata) {
                return offlineAssociationService.fieldValueExpression(fieldMetadata);
            }

            $scope.isFieldHidden = function (fieldMetadata) {
                return fieldService.isFieldHidden($scope.datamap, $scope.schema, fieldMetadata);
            }

            $scope.isFieldRequired = function (requiredExpression) {
                if (Boolean(requiredExpression)) {
                    return expressionService.evaluate(requiredExpression, $scope.datamap);
                }
                return requiredExpression;
            };

        }]
    }
}]);

})(softwrench); 
