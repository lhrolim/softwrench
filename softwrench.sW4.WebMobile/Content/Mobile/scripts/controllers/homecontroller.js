/// <reference path="maincontroller.js" />
(function () {
    'use strict'

    softwrench.controller('HomeController',
        ['$scope', 'synchronizationFacade', '$ionicPopup', '$ionicLoading', 'synchronizationOperationService', 'formatService',
        function($scope, synchronizationFacade, $ionicPopup, $ionicLoading, synchronizationOperationService, formatService) {

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
                    synchronizationOperationService.refreshSync();
                }).catch(function() {
                    $ionicPopup.alert({
                        title: "Error Synchronizing Data",
                    });
                }).finally(function(message) {
                    $ionicLoading.hide();
                });
            };

            $scope.doneNoProblems = function(syncOperation) {
                return syncOperation.status.equalIc("complete") && !synchronizationOperationService.hasProblems(syncOperation);
            };

            $scope.doneWithProblems = function(syncOperation) {
                return syncOperation.status.equalIc("complete") && synchronizationOperationService.hasProblems(syncOperation);
            };

            $scope.isPending = function(syncOperation) {
                return syncOperation.status.equalIc("pending");
            };

            $scope.getItemsSent = function(item) {
                //TODO: get from batch
                return 10;
            };

            $scope.getFormattedDate = function(startdate) {
                return formatService.formatDate(startdate, 'MM/dd/yyyy HH:mm');
            };

            $scope.synchronizationlist = function () {
                // TODO: promise resolution + set number of pending items
                return synchronizationOperationService.getSyncList();
            };

            $scope.getTimeElapsed = function(item) {
                return (item.enddate - item.startdate) / 1000;
            };

            $scope.getFormattedStatus = function(syncOperation) {
                if ($scope.doneNoProblems(syncOperation)) {
                    return "Completed";
                } else if ($scope.isPending(syncOperation)) {
                    return "Pending";
                } else if ($scope.doneWithProblems(syncOperation)) {
                    return "Completed with Issues";
                } else {
                    return undefined;
                }
            };

        }
    ]);

})();