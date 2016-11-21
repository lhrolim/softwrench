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
                    userInterval: $scope.config.intervalConfig.default,
                    timer: $scope.config.intervalConfig.default * $scope.config.intervalConfig.multiplier / 1000
                };

                var currentIntervalPromise = null;
                var currentIntervalDelay = $scope.vm.userInterval;
                var intervalDelayCheckpoint = null;

                function cancelInterval() {
                    if (!!currentIntervalPromise && angular.isFunction(currentIntervalPromise.then)) {
                        $interval.cancel(currentIntervalPromise);
                    }
                }

                $scope.resetInterval = function() {
                    console.log($scope.vm, $scope.config.intervalConfig.default);
                    $scope.vm.userInterval = $scope.config.intervalConfig.default;

                    cancelInterval();
                    scheduleInterval($scope.vm.userInterval);
                }

                function scheduleInterval(delay) {
                    //console.log('scheduleInterval');
                    if (!angular.isNumber(delay) || delay <= 0) {
                        cancelInterval();
                        return;
                    }
                    $scope.vm.activated = true;
                    currentIntervalDelay = delay * $scope.config.intervalConfig.multiplier;
                    $scope.vm.timer = currentIntervalDelay / 1000;

                    $('.interval-trigger .knob').trigger(
                        'configure',
                        {
                            'max': currentIntervalDelay / 1000
                        }
                    );

                    currentIntervalPromise = $interval(function () {
                        $scope.vm.timer--;

                        var scrollKnob = $('.interval-trigger .knob');
                        scrollKnob.val($scope.vm.timer).trigger('change');

                        if ($scope.vm.timer === 0) {
                            $scope.vm.timer = currentIntervalDelay / 1000;
                            $scope.onIntervalTriggered({ delay: currentIntervalDelay });
                        }
                    }, 1000);
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

                $scope.intervalChanged = function (delay) {
                    scheduleInterval(delay);
                    $scope.config.configuring = false;
                };

            }],

            link: function (scope, element, attr) {
                $('.knob', element).knob({
                    'min': 0,
                    'max': 300,
                    'bgColor': '#dedede',
                    'fgColor': '#333',
                    'width': 17,
                    'height': 17,
                    'thickness': .2,
                    'displayInput': false,
                    'readOnly': true
                });
            }
        };

        return directive;
    }]);

})(angular);