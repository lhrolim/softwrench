(function (angular) {
    "use strict";

    function menuRouterService($injector, crudContextService, $q) {
        //#region Utils
        function executeMenuService(menuleaf) {
            const menuService = menuleaf.service;
            const menuMethod = menuleaf.method;
            if (!$injector.has(menuService)) {
                return $q.reject(new Error(`service '${menuService}' not found`));
            }
            const menuServiceInstance = $injector.getInstance(menuService);
            const menuMethodInstance = menuServiceInstance[menuMethod];
            if (!angular.isFunction(menuMethodInstance)) {
                return $q.reject(new Error(`service '${menuService}' has no method '${menuMethod}'`));
            }
            return $q.when(
                menuMethodInstance.apply(menuServiceInstance, [menuleaf])
            );
        }

        //#endregion

        //#region Public methods

        function routeFromMenuItem(menuleaf) {
            switch (menuleaf.type) {
                case "ApplicationMenuItemDefinition":
                    return crudContextService.loadApplicationGrid(menuleaf.application, menuleaf.schema, menuleaf.id, menuleaf.parameters);
                case "ServiceMenuItemDefinition":
                    return executeMenuService(menuleaf);
                default:
                    return $q.reject(new Error(`Unsupported menu type '${menuleaf.type}'`));
            }
        }

        //#endregion

        //#region Service Instance
        const service = {
            routeFromMenuItem
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_mobile_services").factory("menuRouterService", ["$injector", "crudContextService", "$q", menuRouterService]);

    //#endregion

})(angular);