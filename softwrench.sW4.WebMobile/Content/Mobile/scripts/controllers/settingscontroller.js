(function (softwrench) {
    "use strict";

    softwrench.controller("SettingsController", ["$scope", "routeService", "swdbDAO", "contextService", "securityService",
        function ($scope, routeService, swdbDAO, contextService, securityService) {

            function init() {
                var settings = contextService.fetchFromContext("settings", true);
                if (settings) {
                    $scope.settings = settings;
                } else {
                    $scope.settings = {};
                }
            }

            $scope.goToLogin = function () {
                routeService.go("login");
            };

            $scope.save = function () {
                if (!$scope.settings.serverurl.startsWith("http")) {
                    $scope.settings.serverurl = "http://" + $scope.settings.serverurl;
                }

                //TODO: handle settings method correctly, appending http, testing, etc...
                //SWML-39
                swdbDAO.instantiate("Settings", $scope.settings)
                    .then(function (settingsToSave) {
                        
                        return swdbDAO.save(settingsToSave);
                    }).then(function () {
                        contextService.insertIntoContext("settings", $scope.settings);
                        contextService.insertIntoContext("serverurl", $scope.settings.serverurl);
                        // if has an authenticated user go to 'home' (just editting settings)
                        // otherwise go to 'login'
                        var next = securityService.hasAuthenticatedUser() ? "main.home" : "login";
                        routeService.go(next);
                    });
            }

            init();

        }]);

})(softwrench);
