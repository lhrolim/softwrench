(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "swAlertPopup", "routeService", "securityService", "$timeout", "$stateParams", "loadingService", "settingsService",
        function ($scope, swAlertPopup, routeService, securityService, $timeout, $stateParams, loadingService, settingsService) {

            $scope.data = {};

            var showAlert = function(title, message) {
                swAlertPopup.show({
                    title: title,
                    template: message
                });
            }

            const showMessage = function() {
                const message = $stateParams.message;
                if (!!message) {
                    showAlert("Attention", message);
                }
            };

            $scope.login = function (username, password) {
                loadingService.showDefault();

                if (!!username) {
                    $scope.data.username = username;
                }

                if (!!password) {
                    $scope.data.password = password;
                }

                securityService.login($scope.data.username, $scope.data.password)
                    .then(function (data) {
                        routeService.go('main.home');
                        //enforcing SWOFF-93
                        $scope.data = {};
                    })
                    .catch(function (error) {
                        securityService.logout();
                        showAlert("Login failed", !!error && !!error.message ? error.message : "Please check your credentials.");
                    })
                    .finally(function() {
                        loadingService.hide();
                    });
            };

            $scope.viewSettings = function() {
                routeService.go("settings");
            };

            $scope.getIsDemoMode = function () {
                settingsService.getServerUrl().then(function (url) {
                    $scope.isDemoMode = url.indexOf('demo.softwrench.net') > 0;   
                });
            }

            $scope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
                 $timeout(function () {
                     $scope.getIsDemoMode();
                 });
             });

            // init
            $timeout(showMessage);
            $scope.getIsDemoMode();
        }
    ]);

})(softwrench);