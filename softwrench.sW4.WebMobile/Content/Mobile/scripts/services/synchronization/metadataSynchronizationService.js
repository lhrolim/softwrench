mobileServices.factory('metadataSynchronizationService', function ($http, $q, restService, menuModelService, metadataModelService, configurationService) {
    return {

        syncData: function (currentServerVersion) {

            return restService.getPromise("Mobile", "DownloadMetadatas", {}).then(function (metadatasResult) {
                var serverMenu = JSON.parse(metadatasResult.data.menuJson);
                var topLevelMetadatas = JSON.parse(metadatasResult.data.topLevelMetadatasJson);
                var statusColorJson = JSON.parse(metadatasResult.data.statusColorsJSON);
                var associationMetadatasJson = JSON.parse(metadatasResult.data.associationMetadatasJson);
                var compositionMetadatasJson = JSON.parse(metadatasResult.data.compositionMetadatasJson);

                var menuPromise = menuModelService.updateMenu(serverMenu);
                var topLevelPromise = metadataModelService.updateTopLevelMetadata(topLevelMetadatas);
                var associationPromise = metadataModelService.updateAssociationMetadata(associationMetadatasJson);
                var compositionPromise = metadataModelService.updateCompositionMetadata(compositionMetadatasJson);
                //TODO: server return a whole list of configs
                var configServicePromise = configurationService.saveConfigs([{ key: 'statuscolor', value: statusColorJson }]);
                return $q.all([menuPromise, topLevelPromise, associationPromise, compositionPromise, configServicePromise]).then(function (results) {
                    $q.when();
                });
            }).catch(function (errordata) {
                $q.reject();
            });

        }
    }
});