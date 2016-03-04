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



softwrench.directive('crudInputFields', ["$log", "fieldService", "crudContextService", "expressionService", function ($log, fieldService, crudContextService, expressionService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: 'Content/Mobile/templates/directives/crud/crud_input_fields.html',
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
        },

        link: function (scope, element, attrs) {
            scope.name = 'crud_input_fields';

        },

        controller: ["$scope", function ($scope) {
            $scope.getDisplayables = function() {
                return $scope.displayables;
            };

            $scope.getMaxDateValue = function(field) {
                var rendererParameters = field.rendererParameters;
                return (rendererParameters.hasOwnProperty("allowfuture") 
                    && (rendererParameters.allowfuture === "false" || rendererParameters.allowfuture === false)) 
                        ? new Date().toISOString() 
                        : "";
            };

            $scope.getMinDateValue = function(field) {
                return "";
            };

        }]
    }
}]);

})(softwrench); 
