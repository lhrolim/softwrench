(function (mobileServices) {
    "use strict";

    mobileServices.factory("metadataSynchronizationService",
        ["$q", "offlineRestService", "menuModelService", "metadataModelService", "configurationService", "offlineCommandService", "securityService","searchIndexService",
            function ( $q, restService, menuModelService, metadataModelService, configurationService, offlineCommandService, securityService, searchIndexService) {

    var toConfigurationArray = function (configuration) {
        const configArray = Object.keys(configuration).map(k => ({ key: k, value: configuration[k] }));
        return configArray;
    };

    return {
        syncData: function (currentServerVersion) {

            return restService.get("Mobile", "DownloadMetadatas", {}).then(function (metadatasResult) {

                searchIndexService.refreshIndexCaches();

                const serverMenu = JSON.parse(metadatasResult.data.menuJson);
                const topLevelMetadatas = JSON.parse(metadatasResult.data.topLevelMetadatasJson);
                const associationMetadatasJson = JSON.parse(metadatasResult.data.associationMetadatasJson);
                const compositionMetadatasJson = JSON.parse(metadatasResult.data.compositionMetadatasJson);
                const commandBars = JSON.parse(metadatasResult.data.commandBarsJson);
                const config = metadatasResult.data.appConfiguration;

                const menuPromise = menuModelService.updateMenu(serverMenu);
                const topLevelPromise = metadataModelService.updateTopLevelMetadata(topLevelMetadatas);
                const associationPromise = metadataModelService.updateAssociationMetadata(associationMetadatasJson);
                const compositionPromise = metadataModelService.updateCompositionMetadata(compositionMetadatasJson);
                const commandBarsPromise = offlineCommandService.updateCommandBars(commandBars);

                const configArray = toConfigurationArray(config);
                const configServicePromise = configurationService.saveConfigs(configArray);

                return $q.all([menuPromise, topLevelPromise, associationPromise, compositionPromise, commandBarsPromise, configServicePromise]);

            }).then(function (results) {
                //TODO: return whether changes where downloaded or not
                return true;
            });
        }
    }
}]);

})(mobileServices);
