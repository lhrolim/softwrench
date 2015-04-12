/// <reference path="maincontroller.js" />
softwrench.controller('HomeController', function ($scope, routeService,swdbDAO, $http, $ionicPopup) {
    $scope.data = {};

    $scope.fullSynchronize = function () {


        $http.get(routeService.downloadMetadataURL()).success(function (metadatasResult) {
            var menus = JSON.parse(metadatasResult.menuJson);
            $scope.menu.data = menus;
            swdbDAO.instantiate('Menu', $scope.menu).success(function (menuToSave) {
                swdbDAO.save(menuToSave);
            });

            var metadatas = JSON.parse(metadatasResult.metadatasJSON);
        }).error(function(errordata) {
            var alertPopup = $ionicPopup.alert({
                title: 'Error downloading Metadata',
                template: 'Error downloading Metadata'
            });
        });

    }

//    $scope.leftButtons = [{
//        type: 'button-icon icon ion-navicon',
//        tap: function (e) {
//            $scope.toggleMenu();
//        }
//    }];
})