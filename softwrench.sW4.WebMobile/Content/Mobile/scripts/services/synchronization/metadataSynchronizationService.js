mobileServices.factory('metadataSynchronizationService', function ($http, $q, routeService, menuModelService, metadataModelService) {
    return {

        syncData: function (currentServerVersion) {

            var httpPromise = $http.get(routeService.downloadMetadataURL(currentServerVersion));

            httpPromise.then(function (metadatasResult) {
                var serverMenu = JSON.parse(metadatasResult.data.menuJson);
                var topLevelMetadatas = JSON.parse(metadatasResult.data.topLevelMetadatasJson);
                var associationMetadatasJson = JSON.parse(metadatasResult.data.associationMetadatasJson);
                var compositionMetadatasJson = JSON.parse(metadatasResult.data.compositionMetadatasJson);

                var menuPromise = menuModelService.updateMenu(serverMenu);
                var topLevelPromise = metadataModelService.updateTopLevelMetadata(topLevelMetadatas);
                var associationPromise = metadataModelService.updateAssociationMetadata(associationMetadatasJson);
                var compositionPromise = metadataModelService.updateCompositionMetadata(compositionMetadatasJson);


                return $q.all([menuPromise, topLevelPromise, associationPromise, compositionPromise]).then(function (results) {
                    $q.when();
                });
            }).catch(function (errordata) {
                $q.reject();
            });

            return httpPromise;
        }
    }
});