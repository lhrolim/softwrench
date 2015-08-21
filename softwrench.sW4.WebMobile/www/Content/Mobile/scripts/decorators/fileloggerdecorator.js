﻿(function (angular, mobileServices) {
    "use strict";

    /**
     * Decorates $log so that the messages are logged into a file.
     */
    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$log", ["$delegate", "$injector", "rollingLogFileConstants", function ($delegate, $injector, rollingLogFileConstants) {

            //#region Utils

            var $roll;

            var rollingLog = {
                enabled: rollingLogFileConstants.logToFileEnabled,
                configured: false,
                started: false,
                config: {
                    logSize: rollingLogFileConstants.logFileSize,
                    eventBuffer: rollingLogFileConstants.eventBuffer,
                    writeOnPause: rollingLogFileConstants.writeOnPause,
                    console: rollingLogFileConstants.logToConsole,
                    debug: rollingLogFileConstants.debug,
                    prefix: rollingLogFileConstants.logFileName,
                    directory: rollingLogFileConstants.logFileDirectory
                }
            }

            var logMethods = ["log", "info", "warn", "debug", "trace", "error"];

            var startRollingLogService = debounce(function () {
                // deboucing as workaround to the fact that $roll.start is async:
                // multiple $roll.starts called before setting rollingLog.started = true
                // debounce makes all those calls 'turn into' a single one (debounce time determined empirically)
                if (!rollingLog.started) {
                    $roll.start().then(function () {
                        rollingLog.started = true;
                    }).catch(function (error) {
                        console.error("Couldn't start file logger", error);
                    });
                }
            }, 100);

            /**
            * Injects $roll instance and bootstraps it with the default configuration.
            */
            function initRollingLog() {
                if (!rollingLog.enabled) {
                    return;
                }

                // getting around circular deps: $rootScope <- $q <- $roll <- $log <- $exceptionHandler <- $rootScope
                if (!$roll) $roll = $injector.get("$roll");

                if (!rollingLog.configured) {
                    $roll.setConfig(rollingLog.config);
                }
                startRollingLogService();
            }

            function logToFile(level, args) {
                if (!rollingLog.enabled || !rollingLog.started) return;
                $roll[level].apply($roll, args);
            }

            function isLoggingMethod(name) {
                return logMethods.indexOf(name) >= 0;
            }

            /**
             * Enhances the logger's method by logging it's arguments to 
             * a rolling log file using $roll service.
             * 
             * @param Function method a logger's logging method
             * @param String methodName logging method's name
             * @param {} logger instance of a logger service
             */
            function enhanceLoggingMethod(method, methodName, logger) {
                // needs the method reference because if the method is retrieved by name at this point 
                // it leads to a stackoverflow being thrown (infinite recursion enhancing it)
                return function () {
                    var args = [].slice.call(arguments);
                    var logs = method.apply(logger, args);
                    // only log to file if enabled
                    if (logger.isLevelEnabled(methodName) && !!logs && logs.length > 0) {
                        logToFile(methodName, logs);
                    }
                }
            }

            /**
             * Enhances all of the logger's logging methods.
             * 
             * @param {} logger instance of a logger service
             */
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
                initRollingLog();

                var args = [].slice.call(arguments);
                var instance = getInstance.apply($delegate, args);
                enhanceLogger(instance);
                return instance;
            }

            return $delegate;
            //#endregion
        }]);

    }]);


})(angular, mobileServices);