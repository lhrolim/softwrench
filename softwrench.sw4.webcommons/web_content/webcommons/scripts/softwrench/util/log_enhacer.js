//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/

var app = angular.module('sw_layout');
app.run(['$log', 'contextService', enhanceAngularLog]);

function ltEnabledLevel(currLevel, enabledLevel) {
    if (enabledLevel == "none") {
        return true;
    }
    if (enabledLevel == "trace") {
        return false;
    }
    if (enabledLevel == "debug") {
        return currLevel.equalsAny('trace');
    }
    if (enabledLevel == "debug") {
        return currLevel.equalsAny('trace');
    }
    if (enabledLevel == "info") {
        return currLevel.equalsAny('trace', 'debug');
    }
    if (enabledLevel == "warn") {
        return currLevel.equalsAny('trace', 'debug', 'info');
    }
    if (enabledLevel == "error") {
        return currLevel.equalsAny('trace', 'debug', 'info', 'error');
    }
    return true;
}

function getContextLevel(context) {

    var methodLogLevel = sessionStorage["log_" + context];
    if (methodLogLevel !== undefined) {
        return methodLogLevel;
    }
    var indexOf = context.indexOf("#");
    if (indexOf != -1) {
        var serviceName = context.substr(0, indexOf);
        var serviceLogLevel = sessionStorage["log_" + serviceName];
        if (serviceLogLevel !== undefined) {
            return serviceLogLevel;
        }
    }
    return null;
};

function getMinLevel(globalLevel, contextLevel) {
    if (contextLevel == null) {
        return globalLevel;
    }

    if (contextLevel == "trace") {
        return contextLevel;
    }

    if (contextLevel == "debug") {
        return globalLevel == "trace" ? globalLevel : contextLevel;
    }

    if (contextLevel == "info") {
        return globalLevel.equalsAny("trace", "debug") ? globalLevel : contextLevel;
    }

    if (contextLevel == "warn") {
        return globalLevel.equalsAny("trace", "debug", "info") ? globalLevel : contextLevel;
    }

    return globalLevel;
}

function enhanceAngularLog($log, contextService) {
    $log.enabledContexts = [];



    $log.getInstance = function (context) {
        return {
            log: enhanceLogging($log.log, 'log', context, contextService),
            info: enhanceLogging($log.info, 'info', context, contextService),
            warn: enhanceLogging($log.warn, 'warn', context, contextService),
            debug: enhanceLogging($log.debug, 'debug', context, contextService),
            trace: enhanceLogging($log.debug, 'trace', context, contextService),
            error: enhanceLogging($log.error, 'error', context, contextService),
            enableLogging: function (enable) {
                $log.enabledContexts[context] = enable;
            },
            isLevelEnabled: function (level) {
                return isLevelEnabled(level, context);
            }
        };
    };

    function isLevelEnabled(level, context) {
        var enabledLevel = sessionStorage.loglevel;
        if (enabledLevel == undefined) {
            enabledLevel = contextService.retrieveFromContext('defaultlevel');
        }
        var contextLevel = getContextLevel(context);
        enabledLevel = getMinLevel(enabledLevel, contextLevel);

        return !ltEnabledLevel(level, enabledLevel);
    }

    function enhanceLogging(loggingFunc, level, context) {
        return function () {
            var isEnabled = this.isLevelEnabled(level, context);
            if (!isEnabled) {
                return;
            }
            //var modifiedArguments = [].slice.call(arguments);
            //modifiedArguments[0] = [moment().format("dddd hh:mm:ss:SSS a") + '::[' + context + ']> '] + modifiedArguments[0];
            //add timestamp and context to array of arguments (inspect object using the bowser console)
            var currentargs = [].slice.call(arguments);
            var contextarg = [moment().format("dddd hh:mm:ss:SSS a") + '::[' + context + ']> '];
            var modifiedArguments = contextarg.concat(currentargs);

            loggingFunc.apply(null, modifiedArguments);
            if (localStorage.logs == undefined) {
                localStorage.logs = "";
            }
            if (localStorage.logs.length > 10000) {
                //clear to avoid exploding... todo --> send to server side
                localStorage.logs = "";
            }
            localStorage.logs += modifiedArguments[0] + "\n";
        };
    }
}