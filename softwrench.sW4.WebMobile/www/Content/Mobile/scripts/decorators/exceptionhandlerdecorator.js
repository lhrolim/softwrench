(function (angular, mobileServices, cordova) {
    "use strict";

    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$exceptionHandler", ["$delegate", "$injector", "fileConstants", "rollingLogFileConstants", function ($delegate, $injector, fileConstants, rollingLogFileConstants) {
            
            var swAlertPopup, $log, logger, contextService, supportService;

            function lazyInstance(instance, name, factory = $injector, getter = "get") {
                if (!instance) {
                    instance = factory[getter](name);
                }
                return instance;
            }

            function logFilePath() {
                const directory = cordova.file[fileConstants.appDirectory];
                const fileName = rollingLogFileConstants.logFileName;
                return directory + fileName;
            }

            function shouldAlertExceptionLogged(exception) {
                const message = angular.isString(exception) ? exception : exception.message; 
                // only considered a worthwhile exception if has a message with more than 10 characters
                return !!message && message.length >= 10;
            }

            function alertLogLocation() {
                // getting around circular deps: $rootScope <- contextService <- $exceptionHandler <- $rootScop
                contextService = lazyInstance(contextService, "contextService");
                if (!fileConstants.fileEnabled || contextService.isDev()) {
                    return;
                }
                // getting around circular deps: $exceptionHandler <- $interpolate <- $compile <- $ionicTemplateLoader <- $ionicPopup <- swAlertPopup <- $exceptionHandler <- $rootScope
                swAlertPopup = lazyInstance(swAlertPopup, "swAlertPopup");
                
                const path = logFilePath();
                swAlertPopup.show({
                    title: "Unexpected error",
                    template: "Check the application logs in the files " + path
                }, 3000);
            }

            return function (exception, cause) {
                if ((exception && isString(exception) && exception.startsWith("Possibly unhandled rejection:")) || !exception.notifyException) {
                    return;
                }

                // getting around circular deps: $rootScope <- contextService <- $exceptionHandler <- $rootScope
                $log = lazyInstance($log, "$log");
                swAlertPopup = lazyInstance(swAlertPopup, "swAlertPopup");
                logger = lazyInstance(logger, "$exceptionHandler", $log);
                // default behavior (from angular.js source): $log.error.apply($log, arguments);
                logger.error.apply(logger, arguments);
                // alerting log file location for support
                // if (shouldAlertExceptionLogged(exception)) alertLogLocation();
                supportService = lazyInstance(supportService, "supportService");
                const logReportObject = { subject: "Unexpected Error" };
                if (!!exception && !!exception.title) {
                    logReportObject.title = exception.title;
                }
                logReportObject.notifyException = exception.notifyException;
                logReportObject.requestSupportReport = exception.requestSupportReport;
                if (exception.requestSupportReport !== false) {
                    supportService.requestLogReporting(logReportObject);
                } else {
                    swAlertPopup.show({
                        title: exception.title,
                        template: exception.message
                    });
                }
                
            };

        }]);
    }]);


})(angular, mobileServices, cordova);