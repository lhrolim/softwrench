(function (angular, mobileServices) {
    "use strict";

    /**
     * Decorates $log so that the messages are logged into a file.
     */
    mobileServices.config(["$provide", function ($provide) {

        $provide.decorator("$log", ["$delegate", "$injector", function ($delegate, $injector) {

            //#region Utils

            var $roll;

            var logMethods = ["log", "info", "warn", "debug", "trace", "error"];

            function initRollingLog() {
                // getting around circular deps: $log <- $roll <- $log <- $exceptionHandler <- $rootScope
                if (!$roll) {
                    $roll = $injector.get("$roll");
                    // $roll.setConfig( ... );
                }
            }

            function isLoggingMethod(name) {
                return logMethods.indexOf(name) >= 0;
            }

            function enhanceLoggingFn(fn, self) {
                return function () {
                    var args = [].slice.call(arguments);
                    var newargs = ["[Decorated]: "].concat(args);
                    fn.apply(self, newargs);
                }
            }

            function enhanceLogger(logger) {
                for (var method in logger) {
                    if (!logger.hasOwnProperty(method) || !angular.isFunction(logger[method]) || !isLoggingMethod(method)) continue;
                    logger[method] = enhanceLoggingFn(logger[method], logger);
                }
            }

            //#endregion

            //#region Decorator
            var getInstance = $delegate.getInstance;

            $delegate.getInstance = function () {
                var args = [].slice.call(arguments);

                var instance = getInstance.apply(null, args);

                initRollingLog();

                enhanceLogger(instance);

                return instance;
            }

            return $delegate;
            //#endregion
        }]);

    }]);


})(angular, mobileServices);