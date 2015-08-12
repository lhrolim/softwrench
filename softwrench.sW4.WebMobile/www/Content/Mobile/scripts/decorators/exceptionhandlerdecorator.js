(function (angular, mobileServices) {
    "use strict";

    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$exceptionHandler", ["$delegate", "$injector", function ($delegate, $injector) {

            return function (exception, cause) {
                // getting around circular deps: $rootScope <- contextService <- $exceptionHandler <- $rootScope
                var $log = $injector.get("$log");
                var logger = $log.get("$exceptionHandler");
                // default behavior (from angular.js source): $log.error.apply($log, arguments);
                logger.error.apply(logger, arguments);
            };

        }]);
    }]);


})(angular, mobileServices);