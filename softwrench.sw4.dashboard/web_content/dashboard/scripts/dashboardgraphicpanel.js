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
                link: function (scope, element, attrs) {
                    var service = graphicPanelServiceProvider.getService(scope.panel.provider);
                    var graphicContainer = element[0].querySelector(".js_graphic_container");
                    service.authenticate()
                        .then(function (auth) {
                            return service.renderGraphic(graphicContainer, scope.panel, auth);
                        })
                        .then(function(graphic) {
                            scope.graphic = graphic;
                        });
                }
            };

            return directive;
        }]);

})(angular);