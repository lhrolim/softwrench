(function (angular) {
    "use strict";

angular.module('sw_layout')
    .directive('inputModal', function (contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/inputModal.html'),
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            fieldMetadata: '='
        },
        controller: function($scope, $log, $rootScope, formatService, expressionService) {

            $scope.getFormattedValue = function(value, column) {
                return formatService.format(value, column);
            };

            $scope.isbtnLeft = function(fieldMetadata) {
                return true;
            };
            $scope.bindEvalExpression = function(fieldMetadata) {
                if (fieldMetadata.evalExpression == null) {
                    return;
                }
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function(newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope.datamap);
                    }
                });
            }

        }
    };
});

})(angular);