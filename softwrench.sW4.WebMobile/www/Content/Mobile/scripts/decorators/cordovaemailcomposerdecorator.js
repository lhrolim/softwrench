(function (angular, cordova, _) {
    "use strict";

    angular.module("softwrench").config(["$provide", function ($provide) {

        $provide.decorator("$cordovaEmailComposer", ["$delegate", "$q", function ($delegate, $q) {
            //#region Utils
            function getAvailableHandler(defer) {
                return function () {
                    // in some platforms (e.g. Android) the handler receives multiple boolean arguments instead of a single one
                    // here we will consider available if at least one argument is true
                    const available = _.contains(arguments, true);
                    if (available) {
                        defer.resolve(true);
                    } else {
                        // reject with a meaningful error
                        defer.reject(new Error("Email is not available in this device"));
                    }
                }
            }
            /**
             * Fixed isAvailable method: 
             * accounts for multiple return parameters from the plugin's native implementation.
             * 
             * @returns {Promise} 
             */
            function isAvailable() {
                const q = $q.defer();
                cordova.plugins.email.isAvailable(getAvailableHandler(q));
                return q.promise;
            }
            //#endregion

            //#region Decorator
            $delegate.isAvailable = isAvailable.bind($delegate);

            return $delegate;
            //#endregion
        }]);

    }]);

})(angular, cordova, _);