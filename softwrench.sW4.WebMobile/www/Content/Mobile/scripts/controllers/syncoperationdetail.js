(function (softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "routeService", "synchronizationFacade", "swAlertPopup", "$stateParams", "$ionicHistory", "applicationStateService", "$q",
            "$ionicScrollDelegate", "loadingService","attachmentDataSynchronizationService", "metadataModelService", "offlineSchemaService", "crudContextService", 
        function ($scope, service, routeService, synchronizationFacade, swAlertPopup, $stateParams, $ionicHistory, applicationStateService, $q, $ionicScrollDelegate, loadingService, attachmentDataSynchronizationService, metadataModelService, offlineSchemaService, crudContextService) {

            $scope.data = {
                operation: null,
                batchItems: [],
                isLatestOperation: !$stateParams.id,
                isSynching: false,
                currentApplicationState: null,
                applicationStateCollapsed: true,
            }


            var loadSyncOperation = function () {
                var loadPromise = $scope.data.isLatestOperation ? service.getMostRecentOperation() : service.getOperation($stateParams.id);
                return loadPromise.then(function (operation) {
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
                });
            };

            var loadCurrentApplicationState = function () {
                return applicationStateService.currentState().then(function (state) {
                    $scope.data.currentApplicationState = state;
                    angular.forEach(state.applications, appState => {
                        const appMetadata = metadataModelService.getApplicationByName(appState.application);
                        const gridSchema = offlineSchemaService.locateSchema(appMetadata, "list");
                        appState.gridTitle = gridSchema ? crudContextService.gridTitle(gridSchema) : appState.application;
                    });
                });
            };

            var loadData = function (initial) {
                // show loading if initial page load
                if (!!initial) {
                    loadingService.showDefault();
                }
                var operationPromise = loadSyncOperation();
                var currentStatePromise = loadCurrentApplicationState();
                return $q.all([operationPromise, currentStatePromise])
                    .finally(function () {
                        if (!!initial) {
                            loadingService.hide();
                        }
                    });
            };

            $scope.goToHistory = function () {
                routeService.go("main.syncoperationhistory");
            };

            $scope.goBack = function () {
                $ionicHistory.goBack();
            };

            $scope.solveProblems = function () {
                // TODO: resubmit $scope.operation's Batches
            };


            $scope.fullSynchronize = function () {
                $scope.data.isSynching = true;
                loadingService.showDefault();
                synchronizationFacade.fullSync()
                    .then(function (operation) {
                        swAlertPopup.show({
                            title: "Synchronization Succeeded" //TODO: maybe create a message for the popup?
                        });
                        return loadData();
                    })
                    .catch(function (error) {
                        swAlertPopup.show({
                            title: "Error Synchronizing Data",
                            template: !!error && !!error.message ? error.message : ""
                        });
                    })
                    .finally(function () {
                        $scope.data.isSynching = false;
                        loadingService.hide();
                        attachmentDataSynchronizationService.downloadAttachments();
                    });
            };

            $scope.toggleApplicationStateCollapsed = function () {
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