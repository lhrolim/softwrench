(function (softwrench) {
    "use strict";

    softwrench.controller("SyncOperationDetailController",
        ["$scope", "synchronizationOperationService", "contextService", "routeService", "securityService", "synchronizationFacade", "swAlertPopup", "$stateParams", "$ionicHistory", "applicationStateService", "$q", "$timeout", "$ionicPopup",
            "$ionicScrollDelegate", "loadingService", "$ionicPopover", "attachmentDataSynchronizationService", "metadataModelService", "menuModelService", "offlineSchemaService", "crudContextService", "crudContextHolderService", "indexCreatorService", "swdbDAO", "networkConnectionService",
            function ($scope, service, contextService, routeService, securityService, synchronizationFacade, swAlertPopup, $stateParams, $ionicHistory, applicationStateService, $q, $timeout, $ionicPopup, $ionicScrollDelegate, loadingService, $ionicPopover, attachmentDataSynchronizationService, metadataModelService, menuModelService, offlineSchemaService, crudContextService, crudContextHolderService, indexCreatorService, swdbDAO, networkConnectionService) {


                $scope.data = {
                    operation: null,
                    batchItems: [],
                    isLatestOperation: !$stateParams.id,
                    isSynching: false,
                    currentApplicationState: null,
                    applicationStateCollapsed: true,
                    attprogressPercent: 0,
                    attprogressTotal: 0,
                    attprogress: 0
                }



                $scope.$on("sync.attachment.begin", (ev, count) => {
                    $scope.data.attprogressTotal = count;
                })

                $scope.$on("sync.attachment.progress", (ev, increment) => {
                    $scope.data.attprogress += increment;
                    const percent = Math.floor(100 * $scope.data.attprogress / $scope.data.attprogressTotal);
                    $scope.data.attprogressPercent = percent;
                    // $scope.$digest();
                })

                $scope.$on("sync.attachment.end", (ev, count) => {
                    $scope.data.attprogress = 0;
                    $scope.data.attprogressTotal = 0;
                    $scope.data.attprogressPercent = 0;
                })

                const progressData = attachmentDataSynchronizationService.getProgress();
                $scope.data.attprogress = progressData.progress;
                $scope.data.attprogressTotal = progressData.total;

                var progress = function () {
                    return $scope.data.attprogressPercent;
                }

                var shouldShowAttProgress = function () {
                    return $scope.data.attprogressPercent > 0 && $scope.data.attprogressPercent < 0;
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
                    const isFirstSyncPromise = !initial ? $q.when(false) : synchronizationFacade.isFirstSync();
                    return isFirstSyncPromise.then(value => {
                        if (value) {
                            //registering first sync call automatically
                            return $timeout(() => {
                                // $scope.fullSynchronize(true);
                                $scope.showSyncModal();
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
                            return securityService.logout(false, false);
                        }
                        return $q.reject();
                    }).then(() => $scope.fullSynchronize(false));
                }


                $scope.innerFullSynchronize = function (downloadAttachments = true, initial=false) {
                    $scope.data.isSynching = true;
                    loadingService.showDefault();

                    // clears grid search data to consider changes on the metadata
                    crudContextHolderService.clearGridSearch();

                    let needFullResync = false;
                    return synchronizationFacade.fullSync(downloadAttachments)
                        .then(function (operation) {
                            return synchronizationFacade.shouldFullResync(!initial).then((needsResync) => {
                                needFullResync = needsResync;
                                if (!needFullResync) {
                                    swAlertPopup.show({
                                        title: "Synchronization Succeeded" //TODO: maybe create a message for the popup?
                                    });
                                }
                            }).then(() => loadData());
                        })
                        .catch(function (error) {
                            synchronizationFacade.handleError(error);
                        })
                        .finally(function () {
                            $scope.data.isSynching = false;
                            loadingService.hide();
                            if (downloadAttachments) {
                                attachmentDataSynchronizationService.downloadAttachments();
                            }
                            indexCreatorService.createIndexAfterFirstSync();
                            menuModelService.updateAppsCount();
                            if (!!contextService.get("restartneeded")) {
                                //only if there are dynamic scripts loaded
                                contextService.deleteFromContext("restartneeded");
                                window.restartApplication();
                                return;
                            }
                        }).then(() => needFullResync ? securityService.logout(false, false) : $q.reject()).then(() => $scope.innerFullSynchronize(downloadAttachments));
                }

                $scope.showSyncModal = function (initial) {

                    if (attachmentDataSynchronizationService.getProgress().progress !=0){
                        swAlertPopup.show({
                            template: "Please sync after attachment download is complete."
                        });
                        return;
                    }


                    $scope.popupdata= {
                        downloadAttachments : false
                    };

                    $ionicPopup.show({
                        templateUrl: getResourcePath("Content/Mobile/templates/syncmenu.html"),
                        title: 'Sync Options',
                        cssClass: 'syncmodal',
                        scope: $scope,
                        buttons: [{
                          text: 'Sync',
                          type: 'button-positive',
                          onTap: function(e) {
                            $scope.fullSynchronize($scope.popupdata.downloadAttachments, initial)              
                          }
                        }]
                      });


                    // $scope.syncMenuPopOver.show($event);
                }

                $scope.fullSynchronize = function (downloadAttachments = true, initial = false) {
                    synchronizationFacade.shouldFullResync(true, !initial).then((needsResync) => {
                        if (needsResync) {
                            return securityService.logout(false, false).then(() => $scope.innerFullSynchronize(downloadAttachments));
                        }
                        return $scope.innerFullSynchronize(downloadAttachments, initial);
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