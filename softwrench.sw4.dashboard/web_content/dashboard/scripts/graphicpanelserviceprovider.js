(function (angular) {
    "use strict";

    function graphicPanelServiceProvider($injector) {
        //#region Utils
        var servicesCache = {};
        //#endregion

        //#region Public methods
        function getService(provider) {
            if (!provider) return null;
            const providerName = provider.toLowerCase();
            var instance = servicesCache[providerName];
            const serviceName = providerName + "GraphicPanelService";
            if (!instance) {
                instance = $injector.get(serviceName);
                servicesCache[providerName] = instance;
            }
            if (!instance) {
                throw new Error(`No Graphic Panel Service '${serviceName}' found for graphic provider '${providerName}'`);
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
    angular.module("sw_layout").service("graphicPanelServiceProvider", ["$injector", graphicPanelServiceProvider]);
    //#endregion

})(angular);
