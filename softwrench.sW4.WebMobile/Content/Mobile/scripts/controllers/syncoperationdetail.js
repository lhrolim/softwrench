(function(softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "routeService", "synchronizationFacade", "$ionicPopup", "$ionicLoading", "$stateParams", "$ionicHistory",
        function ($scope, service, routeService, synchronizationFacade, $ionicPopup, $ionicLoading, $stateParams, $ionicHistory) {

            $scope.data = {
                operation: null,
                batchItems: [],
                isLatestOperation: !$stateParams.id,
                isSynching: false
            }

            var loadingOptions = {
                //template: "<i class='icon ion-looping'></i> Loading", -> ionicon-animations not added; using spinner instead
                template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>",
                animation: "fade-in"
            };

            var loadSyncOperation = function (initial) {
                // show loading if initial page load
                if (!!initial) {
                    $ionicLoading.show(loadingOptions);
                }
                var loadPromise = $scope.data.isLatestOperation ? service.getMostRecentOperation() : service.getOperation($stateParams.id);
                loadPromise.then(function (operation) {
                    if (!operation) return operation;
                    $scope.data.operation = operation;
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
                    $scope.data.batchItems = items;
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
                $scope.data.isSynching = true;
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
                        $scope.data.isSynching = false;
                    });
            };

            // initialize
            loadSyncOperation(true);

    }]);

})(softwrench);