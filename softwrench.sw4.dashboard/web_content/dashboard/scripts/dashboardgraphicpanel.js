(function (angular) {
    "use strict";

    angular
        .module("sw_layout")
        .directive("dashboardGraphicPanel", ["contextService", "graphicPanelServiceProvider", function (contextService, graphicPanelServiceProvider) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Shared/dashboard/templates/dashboardgraphicpanel.html"),
                scope: {
                    panel: "="
                },
                controller: ["$scope", "graphicPanelServiceProvider", function ($scope, graphicPanelServiceProvider) {
                    $scope.data = {
                        hasError: false,
                        graphic: null
                    };
                    var panel = $scope.panel;
                    var service = graphicPanelServiceProvider.getService(panel.provider);

                    $scope.loadGraphic = function (container) {
                        return service.loadGraphic(container || $scope.data.container, panel)
                            .then(function (graphic) {
                                $scope.data.graphic = graphic;
                                $scope.data.hasError = false;
                            })
                            .catch(function() {
                                $scope.data.hasError = true;
                            });
                    };

                }],
                link: function (scope, element, attrs) {
                    var container = element[0].querySelector(".js_graphic_container");
                    scope.data.container = container;

                    scope.loadGraphic(container);
                }
            };

            return directive;
        }]);

})(angular);