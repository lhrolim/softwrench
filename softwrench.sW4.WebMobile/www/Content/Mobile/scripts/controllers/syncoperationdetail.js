(function(softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "routeService", "synchronizationFacade", "$ionicPopup", "$ionicLoading", "$stateParams", "$ionicHistory", "applicationStateService", "$q", "$ionicScrollDelegate",
        function ($scope, service, routeService, synchronizationFacade, $ionicPopup, $ionicLoading, $stateParams, $ionicHistory, applicationStateService, $q, $ionicScrollDelegate) {

            $scope.data = {
                operation: null,
                batchItems: [],
                isLatestOperation: !$stateParams.id,
                isSynching: false,
                currentApplicationState: null,
                applicationStateCollapsed: true,
            }

            var loadingOptions = {
                //template: "<i class='icon ion-looping'></i> Loading", -> ionicon-animations not added; using spinner instead
                template: "<ion-spinner icon='spiral'></ion-spinner><br><span>Loading<span>",
                animation: "fade-in"
            };

            var loadSyncOperation = function () {
                var loadPromise = $scope.data.isLatestOperation ? service.getMostRecentOperation() : service.getOperation($stateParams.id);
                return loadPromise.then(function(operation) {
                    if (!operation) return operation;
                    $scope.data.operation = operation;
                    return service.getBatchItems(operation);
                }).then(function(items) {
                    if (!items) return items;
                    return items.map(function(item) {
                        if (item.problem) {
                            item.simpleproblem = { message: item.problem.message };
                        }
                        return item;
                    });
                }).then(function(items) {
                    $scope.data.batchItems = items;
                });
            };

            var loadCurrentApplicationState = function() {
                return applicationStateService.currentState().then(function(state) {
                    $scope.data.currentApplicationState = state;
                });
            };

            var loadData = function(initial) {
                // show loading if initial page load
                if (!!initial) {
                    $ionicLoading.show(loadingOptions);
                }
                var operationPromise = loadSyncOperation();
                var currentStatePromise = loadCurrentApplicationState();
                return $q.all([operationPromise, currentStatePromise])
                    .finally(function () {
                        if (!!initial) {
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
                    .then(function (operation) {
                        $ionicPopup.alert({
                            title: "Synchronization Succeeded" //TODO: maybe create a message for the popup?
                        });
                        return loadData();
                    })
                    .catch(function () {
                        $ionicPopup.alert({
                            title: "Error Synchronizing Data"
                        });
                    })
                    .finally(function () {
                        $scope.data.isSynching = false;
                        $ionicLoading.hide();
                    });
            };

            $scope.toggleApplicationStateCollapsed = function() {
                $scope.data.applicationStateCollapsed = !$scope.data.applicationStateCollapsed;
                if ($scope.data.applicationStateCollapsed) {
                    $ionicScrollDelegate.scrollTop(true);
                } else {
                    $ionicScrollDelegate.scrollBottom(true);
                }
            }

            // initialize
            loadData(true);

    }]);

})(softwrench);