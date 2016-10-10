//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/

(function(angular, modules) {
    "use strict";

//var app = angular.module('sw_layout');
//modules.webcommons.run(['$log', 'contextService', enhanceAngularLog]);

/**
 * Enhancing $log with angular decorator so it can be further decorated with other decorators
 * (altering the singleton in 'module.run' makes it harder to chain decorators).
 */
modules.webcommons.config(["$provide", function ($provide) {
    $provide.decorator("$log", ["$delegate", "$injector", function($delegate, $injector) {
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
        return currLevel.equalsAny('trace', 'debug', 'info','warn');
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

    var contextService;

   
    function enhanceLogging(loggingFunc, level, context, relatedContexts) {
        return function () {
            const isEnabled = isLevelEnabled(level, context, relatedContexts);
            if (!isEnabled) {
                return [];
            }
            const currentargs = [].slice.call(arguments);
            const contextarg = [`[${level.toUpperCase()}] ${window.moment().format("dddd hh:mm:ss:SSS a")}::[${context}]> `];
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
            log: enhanceLogging($log.log, 'log', context,relatedContexts, contextService),
            info: enhanceLogging($log.info, 'info', context,relatedContexts, contextService),
            warn: enhanceLogging($log.warn, 'warn', context,relatedContexts, contextService),
            debug: enhanceLogging($log.debug, 'debug', context,relatedContexts, contextService),
            trace: enhanceLogging($log.debug, 'trace', context,relatedContexts, contextService),
            error: enhanceLogging($log.error, 'error', context,relatedContexts, contextService),
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
