var app = angular.module('sw_layout');

app.directive('inputModal', function(contextService) {
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

            $scope.isbtnLeft = function (fieldMetadata) {
                if (fieldMetadata.rendererParameters.btnposition == 'left') {
                    return true;
                }
                return false;
            };

            $scope.click = function () {
                
                alert("Button Clicked");
                return;
            }
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
    }
});