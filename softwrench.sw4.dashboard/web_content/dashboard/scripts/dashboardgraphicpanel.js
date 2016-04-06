(function (angular) {
    "use strict";

    angular
        .module("sw_layout")
        .directive("dashboardGraphicPanel", ["$window", "contextService", function ($window, contextService) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Shared/dashboard/templates/dashboardgraphicpanel.html"),
                scope: {
                    panel: "=",
                    dashboardid: "="
                },
                controller: ["$scope", "graphicPanelServiceProvider", "spinService", "$timeout", function ($scope, graphicPanelServiceProvider, spinService, $timeout) {
                    var service = graphicPanelServiceProvider.getService($scope.panel.provider);

                    $scope.data = {
                        hasError: false,
                        graphic: null
                    };

                    function spinOptions(element) {
                        var width = element.offsetWidth;
                        var height = element.offsetHeight;
                        if (width === 0) width = element.parentElement.offsetWidth;
                        if (height === 0) height = element.parentElement.offsetHeight;
                        return {
                            small: true,
                            top: String(Math.round(height / 2)) + "px",
                            left: String(Math.round(width / 2)) + "px"
                        }
                    }

                    $scope.loadGraphic = function (container) {
                        var element = container || $scope.data.container;
                        var options = spinOptions(element);
                        var spinner = spinService.startSpinner(element, options);
                        return service.loadGraphic(element, $scope.panel)
                            .then(function (graphic) {
                                $scope.data.graphic = graphic;
                                $scope.data.hasError = false;
                            })
                            .catch(function () {
                                $scope.data.hasError = true;
                            })
                            .finally(function () {
                                spinner.stop();
                            });
                    };

                    $scope.handleResize = function(container) {
                        if (!$scope.data.graphic) return;
                        var element = container || $scope.data.container;
                        service.resizeGraphic($scope.data.graphic, element.offsetWidth, element.offsetHeight);
                    };

                    $scope.$on("sw:dashboard:selected", function(event, id) {
                        if ($scope.dashboardid !== id) return;
                        $timeout(function() {
                            service.onDashboardSelected($scope.data.graphic);
                        }, 0, false);
                    });

                    $scope.reloadGraphic = $window.debounce(function() {
                        $($scope.data.container).empty();
                        $scope.loadGraphic();
                    }, 500, true);

                    $scope.$watch("panel", function(newValue, oldValue) {
                        if (newValue === oldValue) return;
                        $scope.reloadGraphic();
                    }, true);

                }],

                link: function (scope, element, attrs) {
                    var container = element[0].querySelector(".js_graphic_container");
                    scope.data.container = container;
                    // handling resize
                    var onWindowResize = $window.debounce(function(e) {
                        scope.handleResize(container);
                    }, 500); // debouncing: webkit does not naturally debounce the resize event
                    angular.element($window).on("resize", onWindowResize);
                    scope.$on("$destroy", function() {
                        angular.element($window).off("resize", onWindowResize);
                    });
                    // loading the graphic
                    scope.loadGraphic(container);
                }
            };

            return directive;
        }]);

})(angular);