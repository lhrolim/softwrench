(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils
        //#endregion

        //#region Public methods
        function getIconClass(item) {
            return laborService.hasItemActiveLabor(item) ? "hasaction" : null;
        }

        function getIconIcon(item) {
            return laborService.hasItemActiveLabor(item) ? "clock-o" : null;
        }
        //#endregion

        //#region Service Instance
        const service = {
            getIconClass,
            getIconIcon
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_mobile_services").factory("laborTimerIconProvider", ["laborService", laborTimerIconProvider]);
    //#endregion

})(angular);
