(function (mobileServices, angular) {
    "use strict";

    mobileServices.factory("configurationService", ["$http", "$log", "$q", "swdbDAO", "contextService", "offlineEntities",
    function ($http, $log, $q, swdbDAO, contextService, entities) {


        /// <summary>
        ///  Load client based configs
        /// </summary>
        /// <returns type=""></returns>
        function loadClientConfigs() {
            var log = $log.get("configurationService#loadSettings");
            return swdbDAO.findAll("Settings").then(function(settings) {
                if (settings.length <= 0) {
                    log.info('creating infos for the first time');
                    var ob = entities.Settings;
                    swdbDAO.save(new ob()).then(function(savedSetting) {
                        contextService.insertIntoContext("settings", savedSetting);
                    });
                } else {
                    log.info('loading settings');
                    contextService.insertIntoContext("settings", settings[0], true);
                    contextService.insertIntoContext("serverurl", settings[0].serverurl);
                }
            });
        };

        /// <summary>
        ///  Load server based configs
        /// </summary>
        /// <returns type=""></returns>
        function loadConfigs() {
            return swdbDAO.findAll("Configuration").then(function (items) {
                for (var i = 0; i < items.length; i++) {
                    var config = items[i];
                    contextService.insertIntoContext(config.key, config.value);
                    if (config.key === "serverconfig") {
                        //adapting so that we can use the same contextService.isDev() here
                        contextService.insertIntoContext("environment", config.value.environment);
                    }
                }
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
                    contextService.insertIntoContext(item.key, item.value);
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