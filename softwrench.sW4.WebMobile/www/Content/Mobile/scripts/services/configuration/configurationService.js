(function (mobileServices, angular) {
    "use strict";

    mobileServices.factory("configurationService", ["$http", "$log", "$q", "swdbDAO", "contextService", "settingsService",
    function ($http, $log, $q, swdbDAO, contextService, settingsService) {

        function updateConfigurationContext(configs) {
            angular.forEach(configs, config => {
                contextService.insertIntoContext(config.key, config.value);
                if (config.key === "serverconfig") {
                    //adapting so that we can use the same contextService.isDev() here
                    contextService.set("environment", config.value.environment);
                }
            });
            return configs;
        }

        /**
         * Load client based configs
         */
        function loadClientConfigs() {
            return settingsService.initializeSettings();
        }

        /**
         * Load server based configs
         */
        function loadConfigs() {
            return swdbDAO.findAll("Configuration").then(updateConfigurationContext);
        }

        function saveConfigs(configs) {
            const entitiesPromises = configs.map(config => swdbDAO.instantiate("Configuration", config));
            return $q.all(entitiesPromises)
                .then(result => swdbDAO.bulkSave(result))
                .then(updateConfigurationContext);
        };

        /**
         * Finds the Configuration with matching key.
         * 
         * @param String key 
         * @returns Promise resolved with the Configuration's value if it was found, null otherwise 
         */
        function getConfig(key) {
            return swdbDAO.findSingleByQuery("Configuration", `key='${key}'`).then(config => !config ? null : config.value);
        }

        const api = {
            loadConfigs,
            loadClientConfigs,
            saveConfigs,
            getConfig
        };
        return api;

    }]);

})(mobileServices, angular);