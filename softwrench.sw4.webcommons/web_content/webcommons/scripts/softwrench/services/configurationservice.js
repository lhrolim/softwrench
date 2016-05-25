(function (angular) {
    "use strict";

    function configurationService(contextService, restService) {
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

        //#endregion

        //#region Service Instance
        const service = {
            getConfigurationValue,
            updateConfigurations
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("configurationService", ["contextService", "restService", configurationService]);

    //#endregion

})(angular);
