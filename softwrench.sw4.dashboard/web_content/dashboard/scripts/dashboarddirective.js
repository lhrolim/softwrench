(function (angular) {
    "use strict";

angular.module("sw_layout").directive("dashboard", ["contextService", function (contextService) {

    return {
        restrict: "E",
        replace: true,
        templateUrl: contextService.getResourceUrl("/Content/Shared/dashboard/templates/dashboarddirective.html"),
        scope: {
            dashboard: "="
        },

        controller: ["$scope", "dashboardAuxService", function ($scope, dashboardAuxService) {

            $scope.getRows = function () {
                if (!$scope.dashboard || !$scope.dashboard.layout) {
                    return 0;
                }
                var arr = [];
                for (var i = 0; i < $scope.dashboard.layout.split(",").length; i++) {
                    arr.push(i);
                }
                return arr;
            };

            $scope.getColumnsOfRow = function (row) {
                if (!$scope.dashboard || !$scope.dashboard.layout) {
                    return 0;
                }
                var colNum = parseInt($scope.dashboard.layout.split(",")[row]);
                var arr = [];
                for (var i = 0; i < colNum; i++) {
                    arr.push(i);
                }
                return arr;
            },

            $scope.getClassByNumberOfColumns = function (row) {
                if (!$scope.dashboard || !$scope.dashboard.layout) {
                    return null;
                }
                var colNum = parseInt($scope.dashboard.layout.split(",")[row]);
                var visibleColumns = 0;
                for (var i = 0; i < colNum; i++) {
                    if (this.isPanelVisible(row, i)) {
                        visibleColumns++;
                    }
                }

                var suffix = 12 / visibleColumns;
                return "col-sm-" + suffix;
            };

            $scope.getPanelDataFromMatrix = function (row, column) {
                return dashboardAuxService.locatePanelFromMatrix($scope.dashboard, row, column);
            };

            $scope.isPanelVisible = function (panelDataSource) {
                return !!panelDataSource && !!panelDataSource.panel && !!panelDataSource.panel.visible;
            };
        }],

        link: function (scope, element, attrs) {
            scope.$name = "dashboardgridsystem";
            scope.dashboardid = scope.dashboard.id;
        }
    };

}]);

})(angular);