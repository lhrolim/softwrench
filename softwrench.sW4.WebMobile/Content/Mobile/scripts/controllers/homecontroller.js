/// <reference path="maincontroller.js" />
(function () {
    "use strict";

    softwrench.controller('HomeController',
        ['$scope', 'synchronizationFacade', '$ionicPopup', '$ionicLoading', 'synchronizationOperationService', 'routeService',
        function($scope, synchronizationFacade, $ionicPopup, $ionicLoading, synchronizationOperationService, routes) {

            $scope.operationList = [];

            $scope.loadSyncOperationList = function () {
                synchronizationOperationService.getSyncList()
                .then(function(operations) {
                    $scope.operationList = operations;
                });
            };

            $scope.fullSynchronize = function() {

                $ionicLoading.show({
                    content: '<i class="icon ion-looping"></i> Loading',
                    animation: 'fade-in',
                    showBackdrop: true,
                    maxWidth: 200,
                    showDelay: 10
                });
                synchronizationFacade.fullSync()
                .then(function (message) {
                    $ionicPopup.alert({
                        title: "Synchronization Suceeded",
                        template: message
                    });
                    $scope.loadSyncOperationList();
                }).catch(function() {
                    $ionicPopup.alert({
                        title: "Error Synchronizing Data",
                    });
                }).finally(function(message) {
                    $ionicLoading.hide();
                });
            };

            $scope.openDetail = function (syncOperation) {
                routes.go("main.syncdetail", { id: syncOperation.id });
            };

            // load syncoperation list after controller is loaded
            $scope.loadSyncOperationList();

            // TODO: infinite scroll loading syncoperation list

        }
    ]);

})();