(function (angular) {
    "use strict";

    angular.module("webcommons_services").directive("intervalTrigger", ["contextService", "userPreferencesService", "$timeout", function (contextService, userPreferencesService, $timeout) {
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
                var timmerActivePreferenceKey = "dashboardTimmerActive";
                var timmerIntervalPreferenceKey = "dashboardTimmerInterval";

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

                var userActive = userPreferencesService.getPreference(timmerActivePreferenceKey);
                var userInterval = userPreferencesService.getPreference(timmerIntervalPreferenceKey);
                var interval = userInterval ? userInterval : $scope.config.intervalConfig.default;
                var timer = interval * $scope.config.intervalConfig.multiplier / 1000;

                $scope.vm = {
                    activated: userActive,
                    userInterval: interval,
                    timer: timer
                };

                var currentIntervalPromise = null;
                var currentIntervalDelay = $scope.vm.userInterval;

                if ($scope.vm.activated) {
                    scheduleInterval($scope.vm.userInterval);
                }

                function cancelInterval() {
                    if (!!currentIntervalPromise) {
                        $interval.cancel(currentIntervalPromise);
                    }
                }

                $scope.resetInterval = function() {
                    $scope.vm.userInterval = $scope.config.intervalConfig.default;

                    cancelInterval();
                    scheduleInterval($scope.vm.userInterval);
                }

                function scheduleInterval(delay) {
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

                        var currentIntervalDelay = $scope.vm.userInterval * $scope.config.intervalConfig.multiplier;
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

                    userPreferencesService.setPreference(timmerActivePreferenceKey, activate);
                };

                $scope.intervalChanged = function (delay) {
                    $timeout(function () {
                        scheduleInterval(delay);
                        $scope.config.configuring = false;
                        userPreferencesService.setPreference(timmerIntervalPreferenceKey, delay);
                    });
                };

            }],
            link: function (scope, element, attr) {
                $('.knob', element).knob({
                    'min': 0,
                    'max': scope.vm.userInterval * scope.config.intervalConfig.multiplier / 1000,
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