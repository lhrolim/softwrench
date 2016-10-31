(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("sidepanel", function (contextService) {
        "ngInject";

        return {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Shared/activitystream/templates/sidePanel.html"),
            transclude: true,
            scope: {
                panelid: "@",
                handletitle: "@",
                handleicon: "@",
                handlewidth: "@"
            },
            compile: function(element, attrs, linker) {
                return function (scope, el) {
                    linker(scope, function(clone) {
                        $(el).find(".pane").append(clone);
                    });
                };
            },
            controller: ["$scope", "$http", "$log", "sidePanelService", function ($scope, $http, $log, sidePanelService) {
                var log = $log.getInstance("sw4.crudSearch");

                // register the current side panel on service
                // the next registers need this to know the panel handle top value
                sidePanelService.registerPanel($scope.panelid, $scope.handlewidth);

                // toggles expanded - collapsed
                $scope.toggle = function () {
                    sidePanelService.toggle($scope.panelid);
                }

                // calcs the style of side panel handle
                $scope.handleStyle = function () {
                    var style = {};
                    var ctx = sidePanelService.getContext($scope.panelid);
                    style["top"] = ctx.top + "px";

                    var width = ctx.handleWidth + "px";
                    style["min-width"] = width;
                    style["max-width"] = width;

                    // translates half width and height back to rotate always on the element corner
                    var trasform = "translate(-{0}px, -18px) rotate(-90deg)".format(Math.floor(ctx.handleWidth / 2));
                    style["-ms-transform"] = trasform;
                    style["-webkit-transform"] = trasform;
                    style["-moz-transform"] = trasform;
                    style["-o-transform"] = trasform;
                    style["transform"] = trasform;

                    log.debug("side panel style: top ({0}), handle width ({1}), transform ({2})".format(style["top"], style["min-width"], style["transform"]));
                    return style;
                }

                $scope.expanded = function() {
                    return sidePanelService.isAnyPanelOpened();
                }

                $scope.opened = function () {
                    return sidePanelService.getContext($scope.panelid).opened;
                }

                $scope.hidden = function () {
                    return sidePanelService.getContext($scope.panelid).hidden;
                }

                $scope.deviceType = function () {
                    return DeviceDetect.catagory.toLowerCase();
                };

                $scope.getHandleTitle = function() {
                    var contextTitle = sidePanelService.getContext($scope.panelid).title;
                    if (contextTitle) {
                        return contextTitle;
                    }
                    return $scope.handletitle;
                }

                $scope.getHandleIcon = function () {
                    var contextIcon = sidePanelService.getContext($scope.panelid).icon;
                    if (contextIcon) {
                        return contextIcon;
                    }
                    return $scope.handleicon;
                }

                if (sidePanelService.getExpandedPanelFromPreference() === $scope.panelid && !sidePanelService.isOpened($scope.panelid)) {
                    sidePanelService.toggle($scope.panelid);
                }
            }]
        }
    });

})(angular);