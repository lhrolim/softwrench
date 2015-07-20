(function(softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "routeService", "synchronizationFacade", "$ionicPopup", "$ionicLoading", "$stateParams", "$ionicHistory",
        function ($scope, service, routeService, synchronizationFacade, $ionicPopup, $ionicLoading, $stateParams, $ionicHistory) {

            $scope.operation = null;

            $scope.batchItems = [];

            $scope.isLatestOperation = !$stateParams.id;

            var loadingOptions = {
                content: "<i class='icon ion-looping'></i> Loading",
                animation: "fade-in",
                showBackdrop: true,
                maxWidth: 200,
                showDelay: 10
            };

            var loadSyncOperation = function (initial) {
                // show loading if initial page load
                if (!!initial) {
                    $ionicLoading.show(loadingOptions);
                }
                var loadPromise = $scope.isLatestOperation ? service.getMostRecentOperation() : service.getOperation($stateParams.id);
                loadPromise.then(function (operation) {
                    if (!operation) return operation;
                    $scope.operation = operation;
                    return service.getBatchItems(operation);
                }).then(function (items) {
                    if (!items) return items;
                    return items.map(function (item) {
                        if (item.problem) {
                            item.simpleproblem = { message: item.problem.message };
                        }
                        return item;
                    });
                }).then(function (items) {
                    $scope.batchItems = items;
                })
                .finally(function () {
                    if (initial) {
                        $ionicLoading.hide();
                    }
                });
            };

            $scope.goToHistory = function() {
                routeService.go("main.syncoperationhistory");
            };

            $scope.goBack = function() {
                $ionicHistory.goBack();
            };

            $scope.solveProblems = function() {
                // TODO: resubmit $scope.operation's Batches
            };

            $scope.fullSynchronize = function () {
                $ionicLoading.show(loadingOptions);
                synchronizationFacade.fullSync()
                    .then(function (message) {
                        $ionicPopup.alert({
                            title: "Synchronization Succeeded",
                            template: message
                        });
                        loadSyncOperation();
                    })
                    .catch(function () {
                        $ionicPopup.alert({
                            title: "Error Synchronizing Data"
                        });
                    })
                    .finally(function () {
                        $ionicLoading.hide();
                    });
            };

            // initialize
            loadSyncOperation(true);

    }]);

})(softwrench);