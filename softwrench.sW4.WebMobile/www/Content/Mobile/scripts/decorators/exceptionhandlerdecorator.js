(function (angular, mobileServices, cordova) {
    "use strict";

    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$exceptionHandler", ["$delegate", "$injector", "rollingLogFileConstants", function ($delegate, $injector, rollingLogFileConstants) {
            
            var $ionicPopup, $timeout, $log, logger;

            function alertLogLocation() {
                if (!rollingLogFileConstants.logToFileEnabled) {
                    return;
                }
                // getting around circular deps: $exceptionHandler <- $interpolate <- $compile <- $ionicTemplateLoader <- $ionicPopup <- $exceptionHandler <- $rootScope
                if (!$ionicPopup) $ionicPopup = $injector.get("$ionicPopup");

                var path = cordova.file[rollingLogFileConstants.logFileDirectory];
                if (!path.endsWith("/")) {
                    path += "/";
                }
                path += rollingLogFileConstants.logFileName;
                var popup = $ionicPopup.alert({
                    title: "Unexpected error",
                    template: "Check the application logs in the files " + path
                });
                $timeout(function() {
                    popup.close();
                }, 3000);
            }

            return function (exception, cause) {
                // getting around circular deps: $rootScope <- $timeout <- $exceptionHandler <- $rootScope
                if(!$timeout) $timeout = $injector.get("$timeout");
                // getting around circular deps: $rootScope <- contextService <- $exceptionHandler <- $rootScope
                if (!logger) {
                    $log = $injector.get("$log");
                    logger = $log.get("$exceptionHandler");
                }
                // default behavior (from angular.js source): $log.error.apply($log, arguments);
                logger.error.apply(logger, arguments);
                // alerting log file location for support
                $timeout(alertLogLocation);
            };

        }]);
    }]);


})(angular, mobileServices, cordova);