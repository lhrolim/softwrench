﻿(function (angular) {
    "use strict";

    angular.module("sw_mobile_services").config(["$provide", function ($provide) {

        $provide.decorator("$q", ["$delegate", function ($delegate) {

            var originalDefer = $delegate.defer;

            $delegate.defer = function () {
                // Get the prototype of the promise
                const promiseProto = originalDefer().promise.constructor.prototype;

                // Add the spread method
                Object.defineProperty(promiseProto, "spread", {
                    value: function (resolve, reject) {
                        function spread(data) {
                            return resolve.apply(void 0, data);
                        }

                        return this.then(spread, reject);
                    },
                    writable: true,
                    enumerable: false
                });

                return originalDefer();
            };

            return $delegate;
        }]);
    }]);

})(angular);