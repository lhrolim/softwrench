(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils

        const hasActiveLabor = item => laborService.getActiveLaborParent() === item.id;

        //#endregion

        //#region Public methods

        function getIconClass(item) {
            return hasActiveLabor(item) ? "hasaction" : null;
        }

        function getIconIcon(item) {
            return hasActiveLabor(item) ? "clock-o" : null;
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
