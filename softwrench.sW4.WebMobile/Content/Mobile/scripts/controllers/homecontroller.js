/// <reference path="maincontroller.js" />
(function () {
    'use strict'

    softwrench.controller('HomeController',
        ['$scope', 'synchronizationFacade', '$ionicPopup', '$ionicLoading', 'synchronizationOperationService', 'formatService',
        function($scope, synchronizationFacade, $ionicPopup, $ionicLoading, synchronizationOperationService, formatService) {
            $scope.data = {};
            $scope.expanded = false;

            $scope.fullSynchronize = function() {

                $ionicLoading.show({
                    content: '<i class="icon ion-looping"></i> Loading',
                    animation: 'fade-in',
                    showBackdrop: true,
                    maxWidth: 200,
                    showDelay: 10
                });
                var promise = synchronizationFacade.fullSync();
                promise.then(function(message) {
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

            $scope.synchronizationlist = function() {
                return synchronizationOperationService.getSyncList();
            };

            $scope.getTimeElapsed = function(item) {
                return (item.enddate - item.startdate) / 1000;
            };

            $scope.getStatusColor = function(item) {
                if (this.doneNoProblems(item)) {
                    return "green";
                }
            };

            $scope.toggleExpansion = function() {
                $scope.expanded = !$scope.expanded;
            };

            // new home style
            var now = new Date();
            $scope.operationList = [
                { status: 'Pending', statuscode: 2, startdate: now, items: 9 },
                { status: 'Completed', statuscode: 1, startdate: now },
                { status: 'Completed', statuscode: 1, startdate: now },
                { status: 'Completed with Issues', statuscode: 0, startdate: now },
                { status: 'Completed', statuscode: 1, startdate: now },
                { status: 'Completed', statuscode: 1, startdate: now },
            ];

        }
    ]);

})();