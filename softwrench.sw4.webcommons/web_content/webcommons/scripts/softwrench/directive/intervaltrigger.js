(function (angular) {
    "use strict";

    angular.module("webcommons_services").directive("intervalTrigger", ["contextService", function (contextService) {
        var directive = {
            templateUrl: contextService.getResourceUrl("Content/Shared/webcommons/templates/intervaltrigger.html"),
            restrict: "E",
            scope: {
                onIntervalTriggered: "&",
                activateLabel: "@",
                /**
                 * interval configuration in the form:
                 * {
                 * 'unit': String, // time unit to use, accepts "second"|"minute"|"hour". Defaults to "second".
                 * 'min': Number, // minimum accepted interval value. Defaults to zero if `null` or `less than zero`
                 * 'max': Number, // maximum accepted interval value. Defaults to Number.MAX_SAFE_INTEGER if `null` or `less than zero`
                 * 'default': Number, // default interval value when the component is started. Defaults to min if `null` or `less than zero`
                 * }
                 */
                intervalConfiguration: "="
            },
            controller: ["$scope", "$interval", function ($scope, $interval) {
                var unitMultiplierTable = {
                    'second': 1000,
                    'minute': 60 * 1000,
                    'hour'  : 60 * 60 * 1000
                };

                function normalizeIntervalConfig() {
                    var config = angular.copy($scope.intervalConfiguration) || {};
                    config.unit = !!config.unit && config.unit.equalsAny("second", "minute", "hour") ? config.unit : "second";
                    config.min = angular.isNumber(config.min) && config.min >= 0 ? config.min : 0;
                    config.max = angular.isNumber(config.max) && config.max > 0 ? config.max : Number.MAX_SAFE_INTEGER;
                    config.default = angular.isNumber(config.default) && config.default > 0 ? config.default : config.min;
                    config.multiplier = unitMultiplierTable[config.unit];
                    return config;
                }

                $scope.config = {
                    configuring: false,
                    intervalConfig: normalizeIntervalConfig()
                };

                $scope.vm = {
                    activated: false,
                    userInterval: $scope.config.intervalConfig.default
                };

                var currentIntervalPromise = null;
                var currentIntervalDelay = $scope.vm.userInterval;
                var intervalDelayCheckpoint = null;

                function cancelInterval() {
                    if (!!currentIntervalPromise && angular.isFunction(currentIntervalPromise.then)) {
                        $interval.cancel(currentIntervalPromise);
                    }
                }

                function scheduleInterval(delay) {
                    if (!angular.isNumber(delay) || delay <= 0) {
                        cancelInterval();
                        return;
                    }
                    currentIntervalDelay = delay * $scope.config.intervalConfig.multiplier;
                    currentIntervalPromise = $interval(function() {
                        $scope.onIntervalTriggered({ delay: currentIntervalDelay });
                    }, currentIntervalDelay, 0, false);
                }

                $scope.onActivateChanged = function (activate) {
                    if (!activate) {
                        cancelInterval();
                    } else {
                        scheduleInterval($scope.vm.userInterval);
                    }
                };

                $scope.onConfigurationFlagChanged = function(configure) {
                    if (configure) {
                        intervalDelayCheckpoint = $scope.vm.userInterval;
                        cancelInterval();
                    }
                };

                $scope.intervalConfigCancel = function() {
                    $scope.vm.userInterval = intervalDelayCheckpoint;
                    $scope.config.configuring = false;
                    scheduleInterval($scope.vm.userInterval);
                };

                $scope.intervalChanged = function(delay) {
                    scheduleInterval(delay);
                    $scope.config.configuring = false;
                };

            }]
        };

        return directive;
    }]);

})(angular);