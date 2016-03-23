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
            }
            
        }],

        link: function (scope, element, attrs) {
            scope.$name = "dashboardgridsystem";
            scope.dashboardid = scope.dashboard.id;
        }
    };

}]);

})(angular);