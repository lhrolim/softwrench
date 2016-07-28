(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils

        const hasActiveLabor = item => laborService.getActiveLaborParent() === item.id;

        //#endregion

        //#region Public methods

        function getIconIcon(item) {
            return hasActiveLabor(item) ? "clock-o" : null;
        }

        function getIconColor(item) {
            return hasActiveLabor(item) ?  "green !important" : null;
        }

        function getTextColor(item) {
            return hasActiveLabor(item) ? "white !important" : null;
        }

        //#endregion

        //#region Service Instance
        const service = {
            getIconColor,
            getIconIcon,
            getTextColor
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("laborTimerIconProvider", ["laborService", laborTimerIconProvider]);

    //#endregion

})(angular);