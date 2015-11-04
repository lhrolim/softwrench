(function (angular) {
    "use strict";

    function graphicPanelServiceProvider($injector) {
        //#region Utils
        var servicesCache = {};
        //#endregion

        //#region Public methods
        function getService(provider) {
            if (!provider) return null;
            var providerName = provider.toLowerCase();
            var instance = servicesCache[providerName];
            if (!instance) {
                instance = $injector.get(providerName + "GraphicPanelService");
                servicesCache[providerName] = instance;
            }
            return instance;
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
