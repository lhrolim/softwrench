(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "$ionicPopup", "routeService", "loginService", "$timeout", "$stateParams", "$ionicLoading",
        function($scope, $ionicPopup, routeService, loginService, $timeout, $stateParams, $ionicLoading) {

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

            $scope.login = function () {
                $ionicLoading.show({
                    template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>"
                });
                loginService.login($scope.data.username, $scope.data.password)
                    .then(function (data) {
                        routeService.go('main.home');
                    })
                    .catch(function () {
                        showAlert("Login failed!", "Please check your credentials!");
                    })
                    .finally(function() {
                        $ionicLoading.hide();
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
