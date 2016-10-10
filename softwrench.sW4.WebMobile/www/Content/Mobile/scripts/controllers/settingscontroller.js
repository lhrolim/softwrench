(function (softwrench) {
    "use strict";

    softwrench.controller("SettingsController", ["$scope", "routeService", "securityService", "applicationStateService", "settingsService", "offlinePersitenceBootstrap",
        function ($scope, routeService, securityService, applicationStateService, settingsService, offlinePersitenceBootstrap) {

            $scope.config = null;

            function init() {
                settingsService.getSettings()
                    .then(settings => {
                        $scope.settings = settings || {};
                        if (!$scope.settings.serverurl) {
                            $scope.settings.serverurl = "http://";
                        }
                        return applicationStateService.getAppConfig();
                    })
                    .then(result => {
                        $scope.config = result;
                        var idx;
                        if ($scope.config.client && (idx = $scope.config.client.version.indexOf("#")) > 0) {
                            //jenkins would append the commit number after a hash
                            //implmentation of SWOFF-130, there was no suitable plugins to read the preferences inside the config.xml
                            const originalVersion = $scope.config.client.version;
                            $scope.config.client.version = originalVersion.substring(0, idx);
                            $scope.config.client.commit = originalVersion.substring(idx + 1);
                        } 
                    });
            }

            $scope.goToLogin = function () {
                routeService.go("login");
            };

            $scope.saveDemo = function () {
                $scope.settings.serverurl = 'http://demo.softwrench.net/';
                $scope.save();
            }

            $scope.save = function () {
                if (!$scope.settings.serverurl.startsWith("http")) {
                    $scope.settings.serverurl = `http://${$scope.settings.serverurl}`;
                }

                settingsService.saveSettings($scope.settings).then(settings => {
                    // if has an authenticated user go to 'home' (just editting settings)
                    // otherwise go to 'login'
                    const next = securityService.hasAuthenticatedUser() ? "main.home" : "login";
                    routeService.go(next);
                });                                   
            }

            offlinePersitenceBootstrap.addPersistenceReadyListener({
                persistenceReady : function() {
                    init();
                }
            });
        }]);

})(softwrench);
