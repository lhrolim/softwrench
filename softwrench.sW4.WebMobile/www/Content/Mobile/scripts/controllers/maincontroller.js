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

            var alertPopup = function(config, timeout) {
                var alert = $ionicPopup.alert({
                    title: config.title,
                    template: config.message
                });
                $timeout(function () {
                    alert.close();
                }, timeout || 3000);
            };

            var doLogout = function() {
                return securityService.logout().then(function() {
                    return true;
                }).catch(function() {
                    alertPopup({
                        title: "Logout failed",
                        message: "Unexpected logout error.<br>" +
                                "Please contact support"
                    });
                    return false;
                });
            };

            $scope.logout = function () {
                synchronizationFacade.hasDataToSync()
                    .then(function(has) {
                        if (!has) {
                            // no data to sync: just logout
                            return doLogout();
                        }
                        // has data to sync
                        if (networkConnectionService.isOffline()) {
                            // not online: no logout action will be executed
                            alertPopup({
                                title: "No internet connection detected",
                                message: "You still have data to synchronize.<br>" +
                                        "Please connect to the internet to logout."
                            });
                            return false;
                        }
                        // is online: prompt user for confirmation
                        return $ionicPopup.confirm({
                                title: "Logout",
                                template: "A synchronization will be performed before the logout, but some data might be lost.<br>" +
                                          "A synchronization will be required after the next login.<br>" +
                                          "Are you sure you want to logout?"
                            })
                            .then(function(res) {
                                // ok then sync before logging out, otherwise break promise chain
                                return !res ? res : synchronizationFacade.attempSyncAndContinue({ template: "Do you wish to logout anyway?" });
                            })
                            .then(function(canlogout) {
                                return !canlogout /* sync failed and user did not wish to continue */ 
                                                  ? false 
                                                  /* sync sucessfull or sync failed and user wished to continue anyway */
                                                  : doLogout();
                            });
                    })
                    .then(function(didlogout) {
                        if (didlogout) {
                            // logout was performed -> redirect to login screen
                            routeService.go("login");
                        }
                    })
                    .finally(function() {
                        $ionicSideMenuDelegate.toggleLeft();
                    });

            };

            init();


        }]);

})(softwrench);


