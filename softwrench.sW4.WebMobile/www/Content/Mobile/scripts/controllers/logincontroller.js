(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "swAlertPopup", "routeService", "securityService", "$timeout", "$stateParams", "$ionicLoading",
        function ($scope, swAlertPopup, routeService, securityService, $timeout, $stateParams, $ionicLoading) {

            $scope.data = {};

            var showAlert = function(title, message) {
                swAlertPopup.show({
                    title: title,
                    template: message
                });
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
                securityService.login($scope.data.username, $scope.data.password)
                    .then(function (data) {
                        routeService.go('main.home');
                        //enforcing SWOFF-93
                        $scope.data = {};
                    })
                    .catch(function (error) {
                        showAlert("Login failed", !!error && !!error.message ? error.message : "Please check your credentials.");
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