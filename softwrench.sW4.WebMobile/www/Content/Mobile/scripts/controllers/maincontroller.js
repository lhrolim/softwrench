(function (softwrench) {
    "use strict";

    softwrench.controller('MainController', ["$scope", "routeService", "$ionicSideMenuDelegate", "menuModelService", "crudContextService", "$ionicPopup", "securityService", "$timeout", "synchronizationFacade", "$ionicLoading",
        function ($scope, routeService, $ionicSideMenuDelegate, menuModelService, crudContextService, $ionicPopup, securityService, $timeout, synchronizationFacade, $ionicLoading) {

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
                if (!navigator.onLine) {
                    var alert = $ionicPopup.alert({
                        title: "Logout",
                        template: "No internet connection detected.<br>" +
                            "Please connect to the internet to logout."
                    });
                    $timeout(function () {
                        alert.close();
                    }, 3000);
                }
                var didlogout = false;
                $ionicPopup.confirm({
                    title: "Logout",
                    template: "Any unsynched data will be lost.<br>" +
                        "A synchronization will be required after the next login.<br>" +
                        "Are you sure you want to logout?"
                }).then(function (res) {
                    if (!res) {
                        // continue promise chain
                        return res;
                    }
                    // sync before logging out
                    return synchronizationFacade.hasDataToSync().then(function (has) {
                        if (!has) {
                            // no data to sync: just logout
                            didlogout = true;
                            return securityService.logout();
                        }
                        $ionicLoading.show({
                            template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Synchronizing data<span>"
                        });
                        // try to sync
                        return synchronizationFacade.fullSync()
                            .then(function () {
                                $ionicLoading.hide();
                                // sync successfull: logout
                                didlogout = true;
                                return securityService.logout();
                            }).catch(function () {
                                $ionicLoading.hide();
                                // sync failed: check if user wishes to logout regardless
                                return $ionicPopup.confirm({
                                    title: "Synchronization failed",
                                    template: "Do you wish to logout anyway?"
                                }).then(function (ressync) {
                                    if (ressync) {
                                        // logout
                                        didlogout = true;
                                        return securityService.logout();
                                    }
                                    // continue promise chain
                                    return ressync;
                                });
                            });
                    });
                }).then(function () {
                    if (didlogout) {
                        routeService.go("login");
                    }
                }).finally(function () {
                    $ionicSideMenuDelegate.toggleLeft();
                });
            };

            init();


        }]);

})(softwrench);


