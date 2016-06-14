(function (angular, mobileServices, cordova) {
    "use strict";

    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$exceptionHandler", ["$delegate", "$injector", "fileConstants", "rollingLogFileConstants", function ($delegate, $injector, fileConstants, rollingLogFileConstants) {
            
            var swAlertPopup, $log, logger, contextService;

            function lazyInstance(instance, name, factory, getter) {
                factory = factory || $injector;
                getter = getter || "get";
                if (!instance) instance = factory[getter](name);
                return instance;
            }

            function logFilePath() {
                var path = cordova.file[fileConstants.logFileDirectory];
                if (!path.endsWith("/")) {
                    path += "/";
                }
                path += rollingLogFileConstants.logFileName;
                return path;
            }

            function shouldAlertExceptionLogged(exception) {
                var message = angular.isString(exception) ? exception : exception.message;
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
                swAlertPopup.alert({
                    title: "Unexpected error",
                    template: "Check the application logs in the files " + path
                }, 3000);
            }

            return function (exception, cause) {
                // getting around circular deps: $rootScope <- contextService <- $exceptionHandler <- $rootScope
                $log = lazyInstance($log, "$log");
                logger = lazyInstance(logger, "$exceptionHandler", $log);
                // default behavior (from angular.js source): $log.error.apply($log, arguments);
                logger.error.apply(logger, arguments);
                // alerting log file location for support
                if (shouldAlertExceptionLogged(exception)) alertLogLocation();
            };

        }]);
    }]);


})(angular, mobileServices, cordova);