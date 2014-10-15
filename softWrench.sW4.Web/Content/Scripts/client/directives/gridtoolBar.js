app.directive('gridtoolbar', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            schema: '=',
            mode: '@',
            position: '@'
        },

        controller: function ($scope, commandService,$log, i18NService, $rootScope) {

            $scope.getGridToolbar = function () {
                var schema = $scope.schema;
                return commandService.getBarCommands(schema, $scope.position);
            }

            $scope.url = function(path) {
                return contextService.getResourceUrl(path);
            }

            $scope.commandtooltip = function (command) {
                return i18NService.get18nValue('_bars.gridtop' + command.id, command.tooltip);
            }

        }
    };
});

