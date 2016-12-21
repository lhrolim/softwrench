(function (angular) {
    "use strict";

    function problemService(contextService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function onListLoad() {
            contextService.insertIntoContext('detail.cancel.click', "_SoftwrenchError.list", null);
        }

        //#endregion

        //#region Service Instance
        const service = {
            onListLoad,
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").service("problemService", ["contextService", problemService]);

    //#endregion

})(angular);