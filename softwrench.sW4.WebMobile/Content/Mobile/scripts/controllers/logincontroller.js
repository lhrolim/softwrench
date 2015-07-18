(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "$ionicPopup", "routeService", "loginService", "$timeout", "$stateParams",
        function($scope, $ionicPopup, routeService, loginService, $timeout, $stateParams) {

            $scope.data = {};

            var showAlert = function(title, message) {
                var alertPopup = $ionicPopup.alert({
                    title: title,
                    template: message
                });
                $timeout(function () {
                    alertPopup.close();
                }, 3000);
            }

            var showMessage = function() {
                var message = $stateParams.message;
                if (!!message) {
                    showAlert("Atention", message);
                }
            };

            $scope.login = function() {
                loginService.login($scope.data.username, $scope.data.password)
                    .then(function (data) {
                        routeService.go('main.home');
                    })
                    .catch(function () {
                        showAlert("Login failed!", "Please check your credentials!");
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
