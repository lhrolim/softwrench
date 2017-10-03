//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/



class LogHelp {
    constructor(description) {
        this.description = description;
    }
}

class LogHelper {

    help() {
        const logs = {
            association: new LogHelp("logs association information"),
            composition: new LogHelp("logs composition information"),
            detail: new LogHelp("crud detail related information"),
            filter: new LogHelp("filter related logs"),
            grid: new LogHelp("crud grid related information"),
            init: new LogHelp("logs information about the initialization of the directives/controllers"),
            layout: new LogHelp("logs information related to the layout of the screen"),
            modal: new LogHelp("modal related logs"),
            route: new LogHelp("information about navigation of pages"),
            save: new LogHelp("log for saving entries"),
            search: new LogHelp("logs information about the search process")
        };

        console.log("**************************************************************************************aspects****************************************************************************************************************");
        console.table(logs);

        const fns = {
            debug: new LogHelp("set a log, or an array of logs to debug level."),
            display: new LogHelp("Displays all current log levels"),
            info: new LogHelp("set a log, or an array of logs to info level."),
            reset: new LogHelp("resets specified logs (or all if none specified) to warn level")
            
        }
        console.log("**************************************************************************************functions****************************************************************************************************************");
        console.table(fns);
    }

    level(items,level) {
        if (!items) {
            sessionStorage.loglevel = level;
            return;
        }

        if (items instanceof Array) {
            items.forEach(i => sessionStorage[`log_${i}`] = level);
        } else {
            sessionStorage[`log_${items}`] = level;
        }
    }

    info(items) {
        this.level(items,"info");
    }

    debug(items) {
        this.level(items,"debug");
    }

    trace(items) {
        this.level(items, "trace");
    }

    display() {
        let i = sessionStorage.length;
        const logs = {};
        while (i--) {
            const key = sessionStorage.key(i);
            if (key.startsWith("log_")) {
                logs[key] = new LogHelp(sessionStorage[key]);
            }
        }
        console.table(logs);
    }

    reset(items) {
        sessionStorage.loglevel = 'warn';
        if (!items) {
            let i = sessionStorage.length;
            const logs = {};
            while (i--) {
                const key = sessionStorage.key(i);
                if (key.startsWith("log_")) {
                    sessionStorage[key] = "warn";
                }
            }
            return;
        }

        if (items instanceof Array) {
            items.forEach(i => sessionStorage[`log_${i}`] = 'warn');
        } else {
            sessionStorage[`log_${items}`] = 'warn';
        }
    }
}


window.swlog = new LogHelper();


(function (angular, modules) {
    "use strict";


    /**
     * Enhancing $log with angular decorator so it can be further decorated with other decorators
     * (altering the singleton in 'module.run' makes it harder to chain decorators).
     */
    modules.rootCommons.config(["$provide", function ($provide) {
        $provide.decorator("$log", ["$delegate", "$injector", function ($delegate, $injector) {
            enhanceAngularLog($delegate, $injector);
            return $delegate;
        }]);
    }]);

    function ltEnabledLevel(currLevel, enabledLevel) {
        if (!enabledLevel) {
            return true;
        }

        if (enabledLevel === "none") {
            return true;
        }
        if (enabledLevel.equalsIc("trace")) {
            return false;
        }
        if (enabledLevel.equalsIc("debug")) {
            return currLevel.equalsAny('trace');
        }
        if (enabledLevel.equalsIc("info")) {
            return currLevel.equalsAny('trace', 'debug');
        }
        if (enabledLevel.equalsIc("warn")) {
            return currLevel.equalsAny('trace', 'debug', 'info');
        }
        if (enabledLevel.equalsIc("error")) {
            return currLevel.equalsAny('trace', 'debug', 'info', 'warn');
        }
        return true;
    }

    function getContextLevel(context, relatedContexts) {
        const methodLogLevel = sessionStorage[`log_${context}`];
        if (methodLogLevel) {
            return methodLogLevel;
        }
        var minLevel = "none";
        if (relatedContexts) {
            angular.forEach(relatedContexts, relatedContext => {
                var relatedContextLevel = sessionStorage[`log_${relatedContext}`];
                if (relatedContextLevel && ltEnabledLevel(relatedContextLevel, minLevel)) {
                    minLevel = relatedContextLevel;
                }
            });
        }

        const indexOf = context.indexOf("#");
        if (indexOf >= 0) {
            const serviceName = context.substr(0, indexOf);
            const serviceLogLevel = sessionStorage[`log_${serviceName}`];
            if (serviceLogLevel) {
                return serviceLogLevel;
            }
        }
        return minLevel;
    };

    function getMinLevel(globalLevel, contextLevel) {
        if (!contextLevel) {
            return globalLevel;
        }

        if (contextLevel === "trace") {
            return contextLevel;
        }

        if (contextLevel === "debug") {
            return globalLevel === "trace" ? globalLevel : contextLevel;
        }

        if (contextLevel === "info") {
            return globalLevel.equalsAny("trace", "debug") ? globalLevel : contextLevel;
        }

        if (contextLevel === "warn") {
            return globalLevel.equalsAny("trace", "debug", "info") ? globalLevel : contextLevel;
        }

        return globalLevel;
    }

    function enhanceAngularLog($log, $injector) {
        $log.enabledContexts = [];

        window.rootlogger = $log;


        var contextService;


        function enhanceLogging(loggingFunc, level, context, relatedContexts) {
            return function () {
                const isEnabled = isLevelEnabled(level, context, relatedContexts);
                if (!isEnabled) {
                    return [];
                }
                const currentargs = [].slice.call(arguments);
                const contextarg = [`[${level.toUpperCase()}] ${window.moment().format("DD/MM/YYYY hh:mm:ss:SSS a")}::[${context}]> `];
                const modifiedArguments = contextarg.concat(currentargs);

                loggingFunc.apply(null, modifiedArguments);

                return modifiedArguments;
            };
        }

        function globalLogLevel() {
            const enabledLevel = sessionStorage["loglevel"];
            return !enabledLevel
                ? contextService.retrieveFromContext("defaultlevel") || "warn"
                : enabledLevel;
        }

        function isLevelEnabled(level, context, relatedContexts) {
            var enabledLevel = globalLogLevel();

            const contextLevel = getContextLevel(context, relatedContexts);
            enabledLevel = getMinLevel(enabledLevel, contextLevel);

            return !ltEnabledLevel(level, enabledLevel);
        }

        $log.getInstance = function (context, relatedContexts) {
            // getting around circular deps: $rootScope <- contextService <- $log <- $exceptionHandler <- $rootScope
            if (!contextService) contextService = $injector.get("contextService");
            return {
                log: enhanceLogging($log.log, 'log', context, relatedContexts, contextService),
                info: enhanceLogging($log.info, 'info', context, relatedContexts, contextService),
                warn: enhanceLogging($log.warn, 'warn', context, relatedContexts, contextService),
                debug: enhanceLogging($log.debug, 'debug', context, relatedContexts, contextService),
                trace: enhanceLogging($log.debug, 'trace', context, relatedContexts, contextService),
                error: enhanceLogging($log.error, 'error', context, relatedContexts, contextService),
                enableLogging(enable) {
                    $log.enabledContexts[context] = enable;
                },
                isLevelEnabled(level) {
                    return isLevelEnabled(level, context, relatedContexts);
                },
                get globalLogLevel() {
                    return globalLogLevel();
                }
            };
        };

        $log.get = function (context, relatedContexts) {
            return this.getInstance(context, relatedContexts);
        };

    };



})(angular, modules);
