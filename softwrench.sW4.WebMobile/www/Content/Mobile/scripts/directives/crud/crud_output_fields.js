(function(softwrench) {
    "use strict";
        
softwrench.directive('crudOutputFields', ["$log", "fieldService", "crudContextService", "formatService", function ($log, fieldService, crudContextService, formatService) {

    return {
        restrict: 'E',

        // has to have this configuration otherwise the controller's methods are not accessible by the template
        replace: false,
        transclude: true,

        templateUrl: getResourcePath('Content/Mobile/templates/directives/crud/crud_output_fields.html'),
        scope: {
            displayables: '=',
            datamap:'='
        },

        link: function (scope, element, attrs) {
            scope.name = 'crud_output_fields';
        },

        controller: ["$scope", function ($scope) {

            $scope.getFormattedValue = function (value, column, datamap) {
                return formatService.format(value, column, datamap);
            };


            $scope.getDisplayables = function () {
                return $scope.displayables;
            }

            $scope.isFieldHidden = function (fieldMetadata) {
                return fieldService.isFieldHidden(crudContextService.currentDetailItem(), crudContextService.currentDetailSchema(), fieldMetadata);
            }

        }]
    }
}]);

})(softwrench);