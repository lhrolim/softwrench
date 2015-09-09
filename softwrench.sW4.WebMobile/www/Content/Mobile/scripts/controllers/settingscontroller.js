(function (softwrench) {
    "use strict";

    softwrench.controller("SettingsController", ["$scope", "routeService", "swdbDAO", "contextService", "securityService", "applicationStateService",
        function ($scope, routeService, swdbDAO, contextService, securityService, applicationStateService) {

            $scope.config = null;

            function init() {
                var settings = contextService.fetchFromContext("settings", true, true);
                $scope.settings = settings || {};


                applicationStateService.getAppConfig().then(function (result) {
                    $scope.config = result;
                    var idx;
                    if ($scope.config.client && (idx = $scope.config.client.version.indexOf("#")) > 0) {
                        //jenkins would append the commit number after a hash
                        //implmentation of SWOFF-130, there was no suitable plugins to read the preferences inside the config.xml
                        var originalVersion = $scope.config.client.version;
                        $scope.config.client.version = originalVersion.substring(0, idx);
                        $scope.config.client.commit = originalVersion.substring(idx + 1);
                    } 
                });


            }



            $scope.goToLogin = function () {
                routeService.go("login");
            };

            $scope.save = function () {
                if (!$scope.settings.serverurl.startsWith("http")) {
                    $scope.settings.serverurl = "http://" + $scope.settings.serverurl;
                }

                swdbDAO.instantiate("Settings", $scope.settings)
                    .then(function (settingsToSave) {
                        return swdbDAO.save(settingsToSave);
                    }).then(function (settingsToSave) {
                        $scope.settings = settingsToSave;
                        contextService.insertIntoContext("settings", $scope.settings, true);
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
