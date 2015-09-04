(function (softwrench) {
    "use strict";

    softwrench.controller('MainController', ["$scope", "routeService", "$ionicSideMenuDelegate", "menuModelService", "crudContextService", "$ionicPopup", "securityService", "$timeout", "synchronizationFacade", "networkConnectionService",
        function ($scope, routeService, $ionicSideMenuDelegate, menuModelService, crudContextService, $ionicPopup, securityService, $timeout, synchronizationFacade, networkConnectionService) {

            $scope.data = {};

            function init() {
                //$scope.menuleafs = ;
            }

            $scope.menuleafs = function () {
                return menuModelService.getMenuItens();
            }

            $scope.isList = function () {
                return crudContextService.isList();
            }

            $scope.toggleLeft = function () {
                $ionicSideMenuDelegate.toggleLeft();
            }

            $scope.title = function () {
                return crudContextService.currentTitle();
            }

            $scope.loadApplication = function (menuleaf) {
                crudContextService.loadApplicationGrid(menuleaf.application, menuleaf.title, menuleaf.schema);
                $ionicSideMenuDelegate.toggleLeft();
            }

            $scope.loadAction = function (action) {
                routeService.go(action);
                $ionicSideMenuDelegate.toggleLeft();
            }

            $scope.logout = function () {
                if (networkConnectionService.isOffline()) {
                    var alert = $ionicPopup.alert({
                        title: "Logout",
                        template: "No internet connection detected.<br>" +
                            "Please connect to the internet to logout."
                    });
                    $timeout(function () {
                        alert.close();
                    }, 3000);
                    $ionicSideMenuDelegate.toggleLeft();
                    // not online, no logout action will be executed
                    return;
                }
                $ionicPopup.confirm({
                    title: "Logout",
                    template: "A synchronization will be performed before the logout, but some data might be lost.<br>" +
                        "A synchronization will be required after the next login.<br>" +
                        "Are you sure you want to logout?"
                })
                .then(function (res) {
                    // ok then sync before logging out, otherwise break promise chain
                    return !res ? res : synchronizationFacade.attempSyncAndContinue({ template: "Do you wish to logout anyway?" });
                })
                .then(function (canlogout) {
                    // user did not confirm logout or sync failed and user did not wish to continue
                    if (!canlogout) {
                        return false;
                    }
                    // no sync required or sync sucessfull or sync failed and user wished to continue anyway
                    return securityService.logout().then(function () {
                        return true;
                    });
                })
                .then(function (didlogout) {
                    if (didlogout) {
                        // logout was performed -> redirect to login screen
                        routeService.go("login");
                    }
                })
                .finally(function () {
                    $ionicSideMenuDelegate.toggleLeft();
                });
            };

            init();


        }]);

})(softwrench);


