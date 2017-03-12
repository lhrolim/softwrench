(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils
        function hasActiveLabor(item) {
            if (!item) {
                return false;
            }

            var activeLabor = laborService.getActiveLabor();

            //is labor composition item
            if (!item.application && !!activeLabor) {
                return activeLabor["#localswdbid"] === item["#localswdbid"];   
            }

            return laborService.getActiveLaborParent() === item.id;
        }
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
