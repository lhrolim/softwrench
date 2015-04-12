mobileServices.factory('synchronizationFacade', function ($http, $log,$q, routeService, swdbDAO, $rootScope, synchronizationPushService, synchronizationPullService) {

    return {

        fullSync: function (currentData) {

            var pendingDataPromise = synchronizationPushService.pushPendingData();
            
            var metadataDownloadedPromise = synchronizationPullService.fetchMetadatas(currentData.serverVersion, currentData.menu);

            var dataDownloadedPromise = synchronizationPullService.fetchNewData();

            var associationDataDownloadPromise = synchronizationPullService.fetchAssociationData(currentData.rowstampmap);

            return $q.all([pendingDataPromise, metadataDownloadedPromise, dataDownloadedPromise, associationDataDownloadPromise]).then(function (results) {
                var dataSentResult = results[0];
                if (dataSentResult.error) {
                    return false;
                }
                return true;
            });

        },


        downloadMetadatas: function (currentMenu) {



            $http.get(routeService.downloadMetadataURL()).success(function (metadatasResult) {
                var menus = JSON.parse(metadatasResult.menuJson);
                currentMenu.data = menus;
                swdbDAO.instantiate('Menu', currentMenu).success(function (menuToSave) {
                    swdbDAO.save(menuToSave).success(function () {
                        $rootScope("sync.menu.updated", menuToSave);
                    });
                });

                var metadatas = JSON.parse(metadatasResult.metadatasJSON);
            }).error(function (errordata) {
                var alertPopup = $ionicPopup.alert({
                    title: 'Error downloading Metadata',
                    template: 'Error downloading Metadata'
                });
            });
        }

    }

});