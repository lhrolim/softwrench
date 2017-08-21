(function (angular) {
    "use strict";

angular.module("sw_layout").directive("dashboard", ["contextService", function (contextService) {

    return {
        restrict: "E",
        templateUrl: contextService.getResourceUrl("/Content/Shared/dashboard/templates/dashboarddirective.html"),
        scope: {
            dashboard: "=",
            canEdit: "=",
            onPanelEdit: "&",
            onPanelRemove: "&"
        },

        controller: ["$scope", "$log", function ($scope, $log) {

            $scope.$name = "dashboardgridsystem";
            $scope.dashboardid = $scope.dashboard.id;
            
            $scope.isPanelVisible = function (panelDataSource) {
                return !!panelDataSource && !!panelDataSource.panel && !!panelDataSource.panel.visible;
            };

            $scope.refreshPanel = function(panel) {
                $scope.$broadcast(DashboardEventConstants.RefreshPanel, panel.id);
            }

            $scope.editPanel = function(panel) {
                return $scope.onPanelEdit({ panel: panel, dashboard: $scope.dashboard });
            };

            $scope.removePanel = function(panelDataSource) {
                return $scope.onPanelRemove({ panelDataSource: panelDataSource, dashboard: $scope.dashboard });
            };

            $scope.panelMoved = function(panelDataSource) {
                $log.get("dashboarddirective#panelMoved", ["dashboard"])
                    .debug("positions changed to",
                            $scope.dashboard.panels.map(function (p) { return p.position; }),
                            "by dragging ", panelDataSource);

                for (var i = 0; i < $scope.dashboard.panels.length; i++) {
                    $scope.dashboard.panels[i].position = i;
                }
            };

        }]
    };

}]);

})(angular);