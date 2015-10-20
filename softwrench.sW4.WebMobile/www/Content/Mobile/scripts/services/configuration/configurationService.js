(function (mobileServices, angular) {
    "use strict";

    mobileServices.factory("configurationService", ["$http", "$log", "$q", "swdbDAO", "contextService", "settingsService",
    function ($http, $log, $q, swdbDAO, contextService, settingsService) {

        /**
         * Load client based configs
         */
        function loadClientConfigs() {
            return settingsService.initializeSettings();
        };

        /**
         * Load server based configs
         */
        function loadConfigs() {
            return swdbDAO.findAll("Configuration").then(function (items) {
                angular.forEach(items, function(config) {
                    contextService.insertIntoContext(config.key, config.value);
                    if (config.key === "serverconfig") {
                        //adapting so that we can use the same contextService.isDev() here
                        contextService.set("environment", config.value.environment);
                    }
                });
            });
        };

        function saveConfigs(configs) {
            var entitiesPromises = configs.map(function (config) {
                return swdbDAO.instantiate("Configuration", config);
            });
            return $q.all(entitiesPromises).then(function (result) {
                return swdbDAO.bulkSave(result);
            }).then(function (items) {
                angular.forEach(items, function (item) {
                    contextService.set(item.key, item.value);
                });
                return items;
            });
        };

        /**
         * Finds the Configuration with matching key.
         * 
         * @param String key 
         * @returns Promise resolved with the Configuration's value if it was found, null otherwise 
         */
        function getConfig(key) {
            return swdbDAO.findSingleByQuery("Configuration", "key='{0}'".format(key))
                .then(function (config) {
                    if (!config) {
                        return null;
                    }
                    return config.value;
                });
        };

        var api = {
            loadConfigs: loadConfigs,
            loadClientConfigs: loadClientConfigs,
            saveConfigs: saveConfigs,
            getConfig: getConfig,
        }

        return api;

    }]);

})(mobileServices, angular);