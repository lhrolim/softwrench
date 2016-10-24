﻿(function (angular) {
    "use strict";

    function configurationService($rootScope, $timeout, contextService, restService, crudContextHolderService, compositionService) {
        //#region Utils
        var configsKey = "swconfigs";
        var timestampKey = "swconfigs.timestamp";

        const fallbackRetry = function () {
            contextService.insertIntoContext(timestampKey, null);
            updateConfigurations();
        }
        //#endregion

        //#region Public methods

        // returns undefined if confg not found
        function getConfigurationValue(key) {
            const configs = contextService.get(configsKey, true);
            if (!configs) {
                fallbackRetry();
                return undefined;
            }
            return configs[key];
        }

        function updateConfigurations() {
            const parameters = {
                cacheTimestamp: contextService.get(timestampKey, true)
            };
            const config = { avoidspin: true };
            return restService.getPromise("Configuration", "GetClientSideConfigurations", parameters, config).then(result => {
                const data = result.data;
                if (!data || data === "null") {
                    return null;
                }
                const configs = data["configurations"];
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

        function fetchConfigurations(keys) {
            return restService.get("Configuration", "GetConfigurations", { fullKeys: keys }).then(r => r.data);
        }

        //#endregion

        //#region Service Instance
        const service = {
            getConfigurationValue,
            updateConfigurations,
            loadConfigsComposition,
            fetchConfigurations
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("configurationService", ["$rootScope", "$timeout", "contextService", "restService", "crudContextHolderService", "compositionService", configurationService]);

    //#endregion

})(angular);