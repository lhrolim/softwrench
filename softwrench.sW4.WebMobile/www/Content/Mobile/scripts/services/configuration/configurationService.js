﻿(function(mobileServices, angular) {
    "use strict";

mobileServices.factory("configurationService", ["$http", "$log", "$q", "swdbDAO", "contextService", function ($http, $log, $q, swdbDAO, contextService) {
    return {
        loadConfigs: function() {
            swdbDAO.findAll("Configuration").success(function(items) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    var config = item.data;
                    contextService.insertIntoContext(config.key, config.value);
                }
            });
        },

        saveConfigs: function(configs) {
            var entitiesPromises = configs.map(function(config) {
                return swdbDAO.instantiate("Configuration", config);
            });
            return $q.all(entitiesPromises).then(function(result) {
                return swdbDAO.bulkSave(result);
            }).then(function (items) {
                angular.forEach(items, function(item) {
                    contextService.insertIntoContext(item.key, item.value);
                });
                return items;
            });
        },

        /**
         * Finds the Configuration with matching key.
         * 
         * @param String key 
         * @returns Promise resolved with the Configuration's value if it was found, null otherwise 
         */
        getConfig: function(key) {
            return swdbDAO.findSingleByQuery("Configuration", "key='{0}'".format(key))
                .then(function(config) {
                    if (!config) {
                        return null;
                    }
                    return config.value;
                });
        }
    };

}]);

})(mobileServices, angular);