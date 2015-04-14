mobileServices.factory('metadataSynchronizationService', function ($http, routeService, swdbDAO,dispatcherService) {
    return {

        syncData: function (currentServerVersion) {
            var deferred = dispatcherService.loadBaseDeferred();


            $http.get(routeService.downloadMetadataURL(currentServerVersion)).success(function (metadatasResult) {
                var menus = JSON.parse(metadatasResult.menuJson);
                currentMenu.data = menus;
                swdbDAO.instantiate('Menu', currentMenu).success(function (menuToSave) {
                    swdbDAO.save(menuToSave).success(function () {
                        deferred.resolve();
                    });
                });

                var metadatas = JSON.parse(metadatasResult.metadatasJSON);
            }).error(function (errordata) {
                deferred.reject();
            });

            return deferred.promise;
        }
    }
 });