(function (angular, mobileServices, _) {
    "use strict";

    /**
     * Decorates $log so that the messages are logged into a file.
     */
    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$log", ["$delegate", "$injector", "rollingLogFileConstants", "fileConstants", function ($delegate, $injector, rollingLogFileConstants, fileConstants) {

            //#region Utils

            var $roll;

            /**
             * $roll config + internal state meta
             */
            const rollingLog = {
                enabled: fileConstants.fileEnabled,
                configured: false,
                started: false,
                config: {
                    logSize: rollingLogFileConstants.logFileSize,
                    eventBuffer: rollingLogFileConstants.eventBuffer,
                    writeOnPause: rollingLogFileConstants.writeOnPause,
                    console: rollingLogFileConstants.logToConsole,
                    debug: rollingLogFileConstants.debug,
                    prefix: rollingLogFileConstants.logFileName,
                    directory: fileConstants.appDirectory
                }
            }

            const logMethods = ["log", "info", "warn", "debug", "trace", "error"];

            const startRollingLogService = ionic.debounce(function () {
                // deboucing as workaround to the fact that $roll.start is async:
                // multiple $roll.starts called before setting rollingLog.started = true
                // debounce makes all those calls 'turn into' a single one (debounce time determined empirically)
                if (!rollingLog.started) {
                    $roll.start()
                        .then(() => rollingLog.started = true)
                        .catch(error => console.error("Couldn't start file logger", error));
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

            /**
             * Logs args to file if the level is enabled.
             * 
             * @param {String} level 
             * @param {Array<Object>} args 
             */
            function logToFile(level, args) {
                if (!rollingLog.enabled || !rollingLog.started) return;
                $roll[level].apply($roll, args);
            }

            const isLoggingMethod = name => _.contains(logMethods, name);


            /**
             * Enhances the logger's method by logging it's arguments to 
             * a rolling log file using $roll service.
             * 
             * @param Function method a logger's logging method
             * @param String methodName logging method's name
             * @param {$log#get} logger instance of a logger service
             */
            function enhanceLoggingMethod(method, methodName, logger) {
                // needs the method reference because if the method is retrieved by name at this point 
                // it leads to a stackoverflow being thrown (infinite recursion enhancing it)
                return function () {
                    const args = [].slice.call(arguments);
                    const logs = method.apply(logger, args);
                    // only log to file if enabled
                    if (logger.isLevelEnabled(methodName) && !!logs && logs.length > 0) {
                        logToFile(methodName, logs);
                    }
                    return logs;
                }
            }

            /**
             * Enhances all of the logger's logging methods.
             * 
             * @param {$log#get} logger instance of a logger service
             */
            function enhanceLogger(logger) {
                for (let method in logger) {
                    if (!logger.hasOwnProperty(method) || !angular.isFunction(logger[method]) || !isLoggingMethod(method)) continue;
                    logger[method] = enhanceLoggingMethod(logger[method], method, logger);
                }
            }

            //#endregion

            //#region Decorator

            var getInstance = $delegate.getInstance;
            let contextService = null;

            // to improve performance - sessionStorage["loglevel"] is called thousands of times
            if (sessionStorage.loglevel) {
                window.loglevel = sessionStorage.loglevel;
            }
            Object.defineProperty(sessionStorage, "loglevel", {
                set: (value) => window.loglevel = value,
                get: () => window.loglevel
            }, { enumerable: true, writable: true, configurable: true });

            function globalLogLevel() {
                if (!window.loglevel) {
                    if (!contextService) {
                        contextService = $injector.get("contextService");
                    }
                    window.loglevel = contextService.retrieveFromContext("defaultlevel") || "warn";
                }
                return window.loglevel;
            }

            const logCache = {};

            $delegate.getInstance = function () {
                if (!arguments[1] || arguments[1].indexOf("angular") === -1) {
                    //prevent initing the rolling logs for log calls that are specific to angular initialization logic, and would possibly trigger a circular dependency
                    //let´s init it later
                    initRollingLog();
                }

                const level = globalLogLevel();
                if (!logCache[level]) {
                    logCache[level] = {}
                }
                const cacheKey = arguments[0] + "#" + arguments[1];
                if (logCache[level][cacheKey]) {
                    return logCache[level][cacheKey];
                }

                const args = [].slice.call(arguments);
                const instance = getInstance.apply($delegate, args);
                enhanceLogger(instance);
                logCache[level][cacheKey] = instance;
                return instance;
            }

            return $delegate;
            //#endregion
        }]);

    }]);


})(angular, mobileServices, _);