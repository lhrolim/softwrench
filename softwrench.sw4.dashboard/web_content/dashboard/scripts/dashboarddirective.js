var app = angular.module('sw_layout');

app.directive('dashboard', function ($timeout, $log, $rootScope, $http, contextService, dashboardAuxService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboarddirective.html'),
        scope: {
            dashboard: '=',
        },

        link: function (scope, element, attrs) {
            scope.$name = 'crudbody';

            scope.getRows = function () {
                if (!scope.dashboard || !scope.dashboard.layout) {
                    return 0;
                }
                var arr = [];
                for (var i = 0; i < scope.dashboard.layout.split(',').length;i++) {
                    arr.push(i);
                }
                return arr;
            }

            scope.getColumnsOfRow = function (row) {
                if (!scope.dashboard || !scope.dashboard.layout) {
                    return 0;
                }
                var colNum = parseInt(scope.dashboard.layout.split(',')[row]);
                var arr = [];
                for (var i = 0; i < colNum; i++) {
                    arr.push(i);
                }
                return arr;
            }

            scope.getPanelDataFromMatrix = function (row, column) {
                return dashboardAuxService.locatePanelFromMatrix(scope.dashboard, row, column);
            } 
        },
    }

});