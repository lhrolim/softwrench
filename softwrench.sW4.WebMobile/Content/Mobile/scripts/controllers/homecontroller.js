softwrench.controller('HomeController', function ($scope, routeService, $http, $ionicPopup) {
    $scope.data = {};

    $scope.fullSynchronize = function () {
        $http.get(routeService.syncURL()).success(function(metadatasResult) {
            var menus = metadatasResult.MenuJson;
            var metadatas = metadatasResult.MetadatasJSON;
        }).error(function(errordata) {
            var alertPopup = $ionicPopup.alert({
                title: 'Error downloading Metadata',
                template: 'Error downloading Metadata'
            });
        });

    }
})