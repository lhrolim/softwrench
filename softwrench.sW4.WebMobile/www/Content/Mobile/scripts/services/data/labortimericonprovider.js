(function (angular) {
    "use strict";

    function laborTimerIconProvider(laborService) {
        //#region Utils

        //#endregion

        //#region Public methods

        function getIconClass(item) {
            console.log('getIconClass');

            const activeLaborParent = laborService.getActiveLaborParent();
            return item.id === activeLaborParent ? 'hasaction' : null;
        }

        function getIconIcon(item) {
            const activeLaborParent = laborService.getActiveLaborParent();
            return item.id === activeLaborParent ? 'clock-o' : null;
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