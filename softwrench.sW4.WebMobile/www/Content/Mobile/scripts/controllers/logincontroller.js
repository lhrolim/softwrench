(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "swAlertPopup", "routeService", "securityService", "$timeout", "$stateParams", "loadingService",
        function ($scope, swAlertPopup, routeService, securityService, $timeout, $stateParams, loadingService) {

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
            $scope.login = function () {
                loadingService.showDefault();
                
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

            $scope.settings = function() {
                routeService.go("settings");
            };

            // init
            $timeout(showMessage);
        }
    ]);

})(softwrench);