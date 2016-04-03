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

        controller: ["$scope", function ($scope) {

            function isPanelVisible(panelDataSource) {
                return !!panelDataSource && !!panelDataSource.panel && !!panelDataSource.panel.visible;
            };
            
            function positionComparator(panelA, panelB) {
                return panelA.position - panelB.position;
            }

            $scope.visiblePanels = function() {
                return $scope.dashboard.panels
                    .filter(isPanelVisible)
                    .sort(positionComparator);
            };

            $scope.editPanel = function(panel) {
                return $scope.onPanelEdit({ panel: panel, dashboard: $scope.dashboard });
            };

            $scope.removePanel = function(panelDataSource) {
                return $scope.onPanelRemove({ panelDataSource: panelDataSource, dashboard: $scope.dashboard });
            };

        }],

        link: function (scope, element, attrs) {
            scope.$name = "dashboardgridsystem";
            scope.dashboardid = scope.dashboard.id;
        }
    };

}]);

})(angular);