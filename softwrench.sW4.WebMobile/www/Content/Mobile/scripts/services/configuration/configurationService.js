mobileServices.factory('configurationService', function ($http, $log, $q, swdbDAO, contextService) {
    return {

        loadConfigs: function () {
            swdbDAO.findAll("Configuration").success(function (items) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    var config = item.data;
                    contextService.insertIntoContext(config.key, config.value);
                }
            });
        },


        saveConfigs: function (configs) {
            var entitiesPromises = [];
            for (var i = 0; i < configs.length; i++) {
                var config = configs[i];
                var entityPreparedPromise = swdbDAO.instantiate("Configuration", { key: config.key, value: config.value });
                entitiesPromises.push(entityPreparedPromise);
            }
            return $q.all(entitiesPromises).then(function (result) {
                return swdbDAO.bulkSave(result);
            }).then(function (items) {
                for (i = 0; i < items.length; i++) {
                    var item = items[i];
                    contextService.insertIntoContext(item.key, item.value);
                }
                $q.when();
            });


        }

    }
});