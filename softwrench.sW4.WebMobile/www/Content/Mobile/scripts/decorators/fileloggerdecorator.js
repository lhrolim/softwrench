(function (angular, mobileServices) {
    "use strict";

    /**
     * Decorates $log so that the messages are logged into a file.
     */
    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$log", ["$delegate", "$injector", function ($delegate, $injector) {

            //#region Utils

            var $roll;
            var logToFileEnabled = false;
            var logMethods = ["log", "info", "warn", "debug", "trace", "error"];

            function initRollingLog() {
                if (isRippleEmulator()) return;
                
                // getting around circular deps: $log <- $roll <- $log <- $exceptionHandler <- $rootScope
                if (!$roll) {
                    $roll = $injector.get("$roll");
                    $roll.setConfig({
                        logSize: 10 * 1024 * 1024, // 10MB
                        eventBuffer: 50,
                        writeOnPause: true,
                        console: false,
                        debug: true, // controlled by the caller
                        prefix: "SWOFF"
                        // directory: ""
                    });
                    $roll.start().then(function(success) {
                        logToFileEnabled = true;
                    }).catch(function(error) {
                        console.log("Couldn't start file logger", error);
                    });
                }
            }

            function logToFile(level, args) {
                if (isRippleEmulator() || !logToFileEnabled) return;
                $roll[level].apply($roll, args);
            }

            function isLoggingMethod(name) {
                return logMethods.indexOf(name) >= 0;
            }

            function enhanceLoggingMethod(method, methodName, logger) {
                return function () {
                    var args = [].slice.call(arguments);
                    var newargs = ["[Decorated]: "].concat(args);
                    method.apply(logger, newargs);
                    // only log to file if enabled
                    if (logger.isLevelEnabled(methodName)) {
                        logToFile(newargs);
                    }
                }
            }

            function enhanceLogger(logger) {
                for (var method in logger) {
                    if (!logger.hasOwnProperty(method) || !angular.isFunction(logger[method]) || !isLoggingMethod(method)) continue;
                    logger[method] = enhanceLoggingMethod(logger[method], method, logger);
                }
            }

            //#endregion

            //#region Decorator
            var getInstance = $delegate.getInstance;

            $delegate.getInstance = function () {
                var args = [].slice.call(arguments);

                var instance = getInstance.apply($delegate, args);

                initRollingLog();

                enhanceLogger(instance);

                return instance;
            }

            return $delegate;
            //#endregion
        }]);

    }]);


})(angular, mobileServices);