(function (angular) {
    "use strict";

    function configurationService($rootScope, $timeout, contextService, restService, crudContextHolderService, compositionService) {
        //#region Utils
        var configsKey = "swconfigs";
        var timestampKey = "swconfigs.timestamp";

        var fallbackRetry = function () {
            contextService.insertIntoContext(timestampKey, null);
            updateConfigurations();
        }
        //#endregion

        //#region Public methods

        // returns undefined if confg not found
        function getConfigurationValue(key) {
            var configs = contextService.get(configsKey, true);
            if (!configs) {
                fallbackRetry();
                return undefined;
            }
            return configs[key];
        }

        function updateConfigurations() {
            var parameters = {
                cacheTimestamp: contextService.get(timestampKey, true)
            }
            var config = { avoidspin: true };
            return restService.getPromise("Configuration", "GetClientSideConfigurations", parameters, config).then(function (result) {
                var data = result.data;
                if (!data || data === "null") {
                    return null;
                }
                var configs = data["configurations"];
                contextService.insertIntoContext(configsKey, configs);
                contextService.insertIntoContext(timestampKey, data["cacheTimestamp"]);
                return configs;
            });
        }

        function loadConfigsComposition() {
            var schema = crudContextHolderService.currentSchema();
            var datamap = crudContextHolderService.rootDataMap();
            compositionService.getCompositionList("#properties_", schema, datamap, 1, 10).then(function (result) {
                compositionService.resolveCompositions(result);
            });
        }

        //#endregion

        //#region Service Instance
        var service = {
            getConfigurationValue: getConfigurationValue,
            updateConfigurations: updateConfigurations,
            loadConfigsComposition: loadConfigsComposition
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("configurationService", ["$rootScope", "$timeout", "contextService", "restService", "crudContextHolderService", "compositionService", configurationService]);

    //#endregion

})(angular);
