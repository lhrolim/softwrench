﻿(function(app) {
    "use strict";

    app.controller('LoginController', ["$scope", "$ionicPopup", "routeService", "securityService", "$timeout", "$stateParams", "$ionicLoading",
        function($scope, $ionicPopup, routeService, securityService, $timeout, $stateParams, $ionicLoading) {

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
                securityService.login($scope.data.username, $scope.data.password)
                    .then(function (data) {
                        routeService.go('main.home');
                        //enforcing SWOFF-93
                        $scope.data = {};
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