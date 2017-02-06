(function (angular) {
    "use strict";

    // created to avoid the use of printservice on richtextfields, thus anabling the use of richtexts on sw_prelogin module
    function printAwaitableService() {
        //#region Utils
        const awaitables = [];
        //#endregion

        //#region Public methods
        function registerAwaitable(awaitable) {
            awaitables.push(awaitable);
        }

        function getAwaitables() {
            return awaitables;
        }

        //#endregion

        //#region Service Instance
        const service = {
            registerAwaitable,
            getAwaitables
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_prelogin").service("printAwaitableService", [printAwaitableService]);

    //#endregion

})(angular);