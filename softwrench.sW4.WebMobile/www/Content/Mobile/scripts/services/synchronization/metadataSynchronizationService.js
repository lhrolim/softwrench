(function (mobileServices) {
    "use strict";

    mobileServices.factory("metadataSynchronizationService",
        ["$http", "$q", "offlineRestService", "menuModelService", "metadataModelService", "configurationService",
            function ($http, $q, restService, menuModelService, metadataModelService, configurationService) {

    var toConfigurationArray = function (configuration) {
        var configArray = [];
        for (var key in configuration) {
            if (!configuration.hasOwnProperty(key)) continue;
            configArray.push({ key: key, value: configuration[key] });
        }
        return configArray;
    };

    return {
        syncData: function (currentServerVersion) {

            return restService.get("Mobile", "DownloadMetadatas", {}).then(function (metadatasResult) {
                var serverMenu = JSON.parse(metadatasResult.data.menuJson);
                var topLevelMetadatas = JSON.parse(metadatasResult.data.topLevelMetadatasJson);
                var associationMetadatasJson = JSON.parse(metadatasResult.data.associationMetadatasJson);
                var compositionMetadatasJson = JSON.parse(metadatasResult.data.compositionMetadatasJson);
                var config = metadatasResult.data.appConfiguration;

                var menuPromise = menuModelService.updateMenu(serverMenu);
                var topLevelPromise = metadataModelService.updateTopLevelMetadata(topLevelMetadatas);
                var associationPromise = metadataModelService.updateAssociationMetadata(associationMetadatasJson);
                var compositionPromise = metadataModelService.updateCompositionMetadata(compositionMetadatasJson);

                var configArray = toConfigurationArray(config);
                var configServicePromise = configurationService.saveConfigs(configArray);

                return $q.all([menuPromise, topLevelPromise, associationPromise, compositionPromise, configServicePromise]);

            }).then(function (results) {
                //TODO: return whether changes where downloaded or not
                return true;
            });
        }
    }
}]);

})(mobileServices);
