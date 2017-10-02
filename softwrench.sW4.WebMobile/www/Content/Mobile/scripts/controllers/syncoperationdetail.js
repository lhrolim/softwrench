(function (softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "contextService", "routeService","securityService", "synchronizationFacade", "swAlertPopup", "$stateParams", "$ionicHistory", "applicationStateService", "$q", "$timeout", "$ionicPopup",
            "$ionicScrollDelegate", "loadingService", "attachmentDataSynchronizationService", "metadataModelService", "menuModelService", "offlineSchemaService", "crudContextService", "crudContextHolderService", "indexCreatorService", "swdbDAO", "networkConnectionService",
            function ($scope, service, contextService, routeService, securityService, synchronizationFacade, swAlertPopup, $stateParams, $ionicHistory, applicationStateService, $q, $timeout, $ionicPopup, $ionicScrollDelegate, loadingService, attachmentDataSynchronizationService, metadataModelService, menuModelService, offlineSchemaService, crudContextService, crudContextHolderService, indexCreatorService, swdbDAO, networkConnectionService) {

                $scope.data = {
                    operation: null,
                    batchItems: [],
                    isLatestOperation: !$stateParams.id,
                    isSynching: false,
                    currentApplicationState: null,
                    applicationStateCollapsed: true
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
                    return applicationStateService.currentApplicationsState().then(function (state) {
                        $scope.data.currentApplicationState = state;
                        angular.forEach(state.applications, appState => {
                            const appMetadata = metadataModelService.getApplicationByName(appState.application);
                            const gridSchema = offlineSchemaService.locateSchema(appMetadata, "list");
                            appState.gridTitle = gridSchema ? crudContextService.gridTitle(gridSchema) : appState.application;
                        });
                    });
                };

                var loadData = function (initial) {
                    let isFirstSyncPromise = !initial ? $q.when(false) : synchronizationFacade.isFirstSync();
                    isFirstSyncPromise.then(value => {
                        if (value) {
                            //registering first sync call automatically
                            return $timeout(() => {
                                $scope.fullSynchronize(true);
                            }, 0, false);
                        } else {
                            loadingService.showDefault();
                            const operationPromise = loadSyncOperation();
                            const currentStatePromise = loadCurrentApplicationState();
                            return $q.all([operationPromise, currentStatePromise])
                                .finally(function () {
                                    if (!!initial) {
                                        loadingService.hide();
                                    }
                                });
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

                $scope.reset = function () {
                    if (networkConnectionService.isOffline()) {
                        swAlertPopup.show({
                            title: "error",
                            template: "No internet connection detected. Cannot perform the reset operation"
                        });
                        return;
                    }


                    return $ionicPopup.confirm({
                        title: "Reset",
                        template: [
                            "All unsaved data will be lost",
                            "Proceed anyway?",
                        ].join(" ")
                    }).then((confirmed) => {
                        if (confirmed) {
                            return securityService.logout(false,false);    
                        }
                        return $q.reject();
                    }).then(() => $scope.fullSynchronize());
                }


                $scope.fullSynchronize = function () {
                    $scope.data.isSynching = true;
                    loadingService.showDefault();

                    // clears grid search data to consider changes on the metadata
                    crudContextHolderService.clearGridSearch();

                    return synchronizationFacade.fullSync()
                        .then(function (operation) {
                            swAlertPopup.show({
                                title: "Synchronization Succeeded" //TODO: maybe create a message for the popup?
                            });


                            return loadData();
                        })
                        .catch(function (error) {
                            synchronizationFacade.handleError(error);
                        })
                        .finally(function () {
                            $scope.data.isSynching = false;
                            loadingService.hide();
                            attachmentDataSynchronizationService.downloadAttachments();
                            indexCreatorService.createIndexAfterFirstSync();
                            if (!!contextService.get("restartneeded")) {
                                //only if there are dynamic scripts loaded
                                contextService.deleteFromContext("restartneeded");
                                window.restartApplication();
                                return;
                            }
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