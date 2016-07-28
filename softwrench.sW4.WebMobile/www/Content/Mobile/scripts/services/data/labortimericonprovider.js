(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function getIconIcon(item) {
            const activeLaborParent = laborService.getActiveLaborParent();
            return item.id === activeLaborParent ? "clock-o" : null;
        }

        function getIconColor(item) {
            //const activeLaborParent = laborService.getActiveLaborParent();
            return 'green';
        }

        function getTextColor(item) {
            //const activeLaborParent = laborService.getActiveLaborParent();
            return 'green';
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