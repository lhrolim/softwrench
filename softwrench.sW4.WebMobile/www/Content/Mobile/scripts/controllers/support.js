(function (angular) {
    "use strict";

    angular.module("softwrench")
        .controller("SupportController", ["$scope", "supportService", "$ionicPopup", "dynamicScriptsCacheService", "securityService", "routeService", function ($scope, supportService, $ionicPopup, dynamicScriptsCacheService, securityService, routeService) {

            $scope.vm = {
                loglevel: sessionStorage["loglevel"] || "WARN"
            }

            $scope.changeloglevel = function (callback) {
                sessionStorage.loglevel = callback.item.value;
                if (callback.item.value.equalIc('debug')) {
                    persistence.debug = true;
                } else {
                    persistence.debug = false;
                }
            }

            $scope.sendLogFiles = function () {
                supportService.requestLogReporting();
            };

            $scope.reset = function () {
                const confirm = $ionicPopup.confirm({
                    title: "Reset configurations",
                    template: `Are you sure that you want to reset configurations? All non synchronized work will be lost`
                });
                return confirm.then(res => {
                    dynamicScriptsCacheService.clearEntries();
                    window.location.reload();
                    securityService.logout(true).then(() => {
                        return routeService.go("login");
                    }).then(l => {

                    });
                });
                supportService.requestLogReporting();
            };

        }]);

})(angular);