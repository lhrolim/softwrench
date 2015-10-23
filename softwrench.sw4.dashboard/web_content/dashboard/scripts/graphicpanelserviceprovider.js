(function (angular) {
    "use strict";

    function graphicPanelServiceProvider($injector) {
        //#region Public methods
        function getService(provider) {
            return $injector.get(provider.toLowerCase() + "GraphicPanelService");
        }
        //#endregion

        //#region Service Instance
        var service = {
            getService: getService
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("graphicPanelServiceProvider", ["$injector", graphicPanelServiceProvider]);
    //#endregion

})(angular);
