mobileServices.factory('metadataSynchronizationService', function ($http, $q, routeService, menuModelService, metadataModelService) {
    return {

        syncData: function (currentServerVersion) {

            var defer = $q.defer();

            var httpPromise = $http.get(routeService.downloadMetadataURL(currentServerVersion));
            
            httpPromise.then(function (metadatasResult) {
                var serverMenu = JSON.parse(metadatasResult.data.menuJson);
                var serverMetadata = JSON.parse(metadatasResult.data.metadatasJSON);
                var menuPromise = menuModelService.updateMenu(serverMenu);
                var metadataPromise = metadataModelService.updateMetadata(serverMetadata);
                return $q.all([menuPromise, metadataPromise]).then(function(results) {
                    defer.resolve();
                });
            }).catch(function (errordata) {
                defer.reject();
            });

            return defer.promise;
        }
    }
 });