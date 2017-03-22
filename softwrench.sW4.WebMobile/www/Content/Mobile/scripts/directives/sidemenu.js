(function (angular) {
    "use strict";

    angular.module("softwrench").directive("sideMenu", [ function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/sidemenu.html",
            scope: {},

            controller: [
                "$scope", "menuModelService", "menuRouterService", "routeService", "$ionicSideMenuDelegate", "swAlertPopup", "$ionicPopup", "securityService", "synchronizationFacade", "networkConnectionService",
                function ($scope, menuModelService, menuRouterService, routeService, $ionicSideMenuDelegate, swAlertPopup, $ionicPopup, securityService, synchronizationFacade, networkConnectionService) {

                    $scope.menuleafs = function () {
                        return menuModelService.getApplicationMenuItems();
                    };

                    $scope.loadApplication = function (menuleaf) {
                        menuRouterService.routeFromMenuItem(menuleaf)
                            .catch(e => swAlertPopup.show({ title: "Error", template: e.message }, 3000))
                            .finally(() => $ionicSideMenuDelegate.toggleLeft());
                    };

                    $scope.loadAction = function (action) {
                        routeService.go(action);
                        $ionicSideMenuDelegate.toggleLeft();
                    };

                    $scope.adminMenuItems = function () {
                        return menuModelService.getAdminMenuItems();
                    };

                    $scope.userMenuItems = function () {
                        return menuModelService.getUserMenuItems();
                    };

                    $scope.getAppCount = function(menuId) {
                        return menuModelService.getAppCount(menuId);
                    }

                    //#region logout
                    function doLogout() {
                        return securityService.logout()
                            .then(() => true)
                            .catch(() => {
                                swAlertPopup.show({
                                    title: "Logout failed",
                                    template: "Unexpected logout error.<br>Please contact support."
                                }, 3000);
                                return false;
                            });
                    };

                    $scope.logout = function () {
                        synchronizationFacade.hasDataToSync()
                            .then(has => {
                                if (!has) {
                                    // no data to sync: just logout
                                    return doLogout();
                                }
                                // has data to sync
                                if (networkConnectionService.isOffline()) {
                                    // not online: no logout action will be executed
                                    swAlertPopup.show({
                                        title: "No internet connection detected",
                                        template: [
                                            "You still have data to synchronize",
                                            "Please connect to the internet to logout."
                                        ].join("<br>")
                                    }, 3000);
                                    return false;
                                }
                                // is online: prompt user for confirmation
                                return $ionicPopup.confirm({
                                    title: "Logout",
                                    template: [
                                        "A synchronization will be performed before the logout, but some data might be lost.",
                                        "A synchronization will be required after the next login.",
                                        "Are you sure you want to logout?"
                                    ].join(" ")
                                })
                                .then(res => {
                                    // ok then sync before logging out, otherwise break promise chain
                                    return !res ? res : synchronizationFacade.attempSyncAndContinue({ template: "Do you wish to logout anyway?" });
                                })
                                .then(canlogout => {
                                    return !canlogout /* sync failed and user did not wish to continue */
                                        ? false
                                        /* sync sucessfull or sync failed and user wished to continue anyway */
                                        : doLogout();
                                });
                            })
                            .then(didlogout => {
                                if (didlogout) {
                                    // logout was performed -> redirect to login screen
                                    routeService.go("login");
                                }
                            })
                            .finally(() => {
                                $ionicSideMenuDelegate.toggleLeft();
                            });
                    };
                    //#endregion
                }],
        };

        return directive;
    }]);

})(angular);