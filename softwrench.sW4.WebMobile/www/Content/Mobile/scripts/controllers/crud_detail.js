
(function (softwrench) {
    "use strict";

    softwrench.controller("CrudDetailController", ['$log', '$scope', '$rootScope', '$timeout', 'schemaService', "crudContextHolderService", "wizardService", "$ionicPlatform", 
    'crudContextService', 'fieldService', 'offlineAssociationService', '$ionicPopover', '$ionicPopup', '$ionicHistory', '$ionicScrollDelegate', 'eventService', "expressionService", "offlineSchemaService", "commandBarDelegate", "swAlertPopup",
    function (log, $scope, $rootScope, $timeout, schemaService, crudContextHolderService, wizardService, $ionicPlatform,
    crudContextService, fieldService, offlineAssociationService, $ionicPopover, $ionicPopup, $ionicHistory, $ionicScrollDelegate, eventService, expressionService,offlineSchemaService,commandBarDelegate, swAlertPopup) {
        
        function turnOffChangeEvents() {
            $rootScope.areChangeEventsEnabled = false;
        }

        function turnOnChangeEvents() {
            // to force change the flag after the events are trigged
            $timeout(() => $rootScope.areChangeEventsEnabled = true, 0, false);
        }

        function init() {
            log.get("crud_detail#init").debug("crud detail init");
            $scope.allDisplayables = crudContextService.mainDisplayables();
            $scope.displayables = wizardService.getWizardFields($scope.allDisplayables);
            $scope.schema = crudContextService.currentDetailSchema();
            $scope.datamap = crudContextService.currentDetailItemDataMap();
            $scope.item = crudContextHolderService.currentDetailItem();
            eventService.onload($scope, $scope.schema, $scope.datamap, {});
            $rootScope.areChangeEventsEnabled = true;
        }

        $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
            scope: $scope,
        }).then(function (popover) {
            $scope.compositionpopover = popover;
        });



        $scope.expandCompositions = function ($event) {
            $scope.compositionpopover.show($event);
        }

        $scope.compositionListSchema = null;

        $scope.$on("sw_compositionselected", function () {
            $scope.compositionpopover.hide();
            $scope.compositionListSchema = $scope.isOnMainTab() ? null : crudContextService.getCompositionListSchema();
        });

        $scope.$watch(() => {
            return $scope.isOnMainTab();
        }, (newValue, oldValue) => {
            if (oldValue === newValue) {
                return;
            }
            if (newValue) {
                $scope.compositionListSchema = null;
            } else {
                $scope.compositionListSchema = crudContextService.getCompositionListSchema();
            }
        });

        $scope.loadMainTab = function () {
            $ionicScrollDelegate.scrollTop();
            crudContextService.loadTab(null);
        }

        $scope.tabTitle = function () {
            return crudContextService.tabTitle();
        }


        $scope.showNavigation = function () {
            return $scope.isOnMainTab() && !crudContextService.hasDirtyChanges();
        }

        $scope.hasNextItem = function() {
            return !!crudContextHolderService.getCrudContext().nextItem;
        };

        $scope.hasAnyComposition = function () {
            return crudContextService.currentCompositionsToShow().length > 0;
        }

        $scope.cancelChanges = function () {
            var confirmPopup = $ionicPopup.confirm({
                title: 'Confirm Cancel',
                template: 'Any changes made will be lost. Proceed?'
            });
            confirmPopup.then(function (res) {
                if (res) {
                    turnOffChangeEvents();
                    crudContextService.cancelChanges();
                    $scope.datamap = crudContextService.currentDetailItemDataMap();

                    // to force change the flag after the events are trigged
                    $timeout(() => $rootScope.areChangeEventsEnabled = true, 0, false);
                    turnOnChangeEvents();
                }
            });
        }

        function showValidationErrors(validationErrors) {
            swAlertPopup.alertValidationErrors(validationErrors);
        }

        $scope.saveChanges = function () {
            crudContextService.saveChanges().then(function () {
                init();
            }).catch(showValidationErrors);
        }

        $scope.hasDirtyChanges = function () {
            return crudContextService.hasDirtyChanges();
        }

        $scope.shouldShowBack = function () {
            return !this.hasDirtyChanges() && $ionicHistory.viewHistory().backView;
        }

        $scope.navigateBack = function () {
            return $ionicHistory.goBack();
        }

        $scope.isOnMainTab = function () {
            return crudContextService.isOnMainTab();
        }

        $scope.detailSubTitle = function () {
            if (crudContextService.isCreation()) {
                return null;
            }
            return offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "subtitle", $scope.datamap);
        }

        $scope.detailFeatured = function (item) {
            if (crudContextService.isCreation()) {
                return null;
            }
            return offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "featured", $scope.datamap);
        }

        $scope.detailTitle = function () {
            const datamap = crudContextService.isCreation() ? null : $scope.datamap; // workaround to force new title
            return schemaService.getTitle(crudContextService.currentDetailSchema(), datamap, true);
        }

        $scope.detailSummary = function () {
            const datamap = crudContextService.isCreation() ? null : $scope.datamap; // workaround to force new title
            return schemaService.getSummary(crudContextService.currentDetailSchema(), datamap, true);
        }

        const arrowNavigate = function(navigate) {
            turnOffChangeEvents();
            crudContextService[navigate]().then(function () {
                $scope.datamap = crudContextService.currentDetailItemDataMap();
            }).finally(() => {
                turnOnChangeEvents();
            });
        }

        $scope.navigateNext = function () {
            arrowNavigate("navigateNext");
        }

        $scope.navigatePrevious = function () {
            arrowNavigate("navigatePrevious");
        }

        $scope.onSwipeLeft = function() {
            if ($scope.showNavigation()) {
                $scope.navigateNext();
            }
        };

        $scope.onSwipeRight = function() {
            if ($scope.showNavigation()) {
                $scope.navigatePrevious();
            }
        };

        $scope.onScroll = function () {
            const position = $ionicScrollDelegate.$getByHandle('detailHandler').getScrollPosition();
            //update the position of the detail's floating action button when the user scrolls
            if (!!position) {
                const element = $("command-bar[position=\"mobile.fab\"]");
                commandBarDelegate.positionFabCommandBar(element, position.top);
            }
        };

        $scope.shouldShowWizardBack = function() {
            return wizardService.canReturn();
        }

        $scope.shouldShowWizardForward = function () {
            return wizardService.isInWizardState($scope.allDisplayables);
        }

        $scope.wizardNavigateBack = function () {
            $scope.displayables = wizardService.previous($scope.allDisplayables);
        }

        $scope.wizardNavigateForward = function () {
            const validationErrors = crudContextService.validateDetail({},null, $scope.displayables);
            if (validationErrors.length !== 0) {
                showValidationErrors(validationErrors);
                return;
            }
            $scope.displayables = wizardService.next($scope.allDisplayables);
        }

        $scope.isDirty = function (item) {
            return $scope.item.isDirty;
        }

        $scope.isPending = function (item) {
            return $scope.item.pending;
        }

        $scope.hasProblems = function () {
            return $scope.item.hasProblem;
        }

        $scope.lastProblemMesage = function() {
            const problems = crudContextHolderService.currentProblems();
            if (!problems || problems.length === 0) {
                return null;
            }
            return problems[0].message;
        }


        // handles device back button
        const deregisterHardwareBack = $ionicPlatform.registerBackButtonAction(() => {
            if ($scope.shouldShowBack()) {
                $scope.navigateBack();
            } else if ($scope.shouldShowWizardBack()) {
                $scope.wizardNavigateBack();
                $scope.$apply();
            } else{
                $scope.cancelChanges();
            }
        }, 100);
        $scope.$on("$destroy", deregisterHardwareBack);

        $scope.$on('sw_cruddetailrefreshed', function () {
            $scope.datamap = crudContextService.currentDetailItemDataMap();
        });

        $scope.$on('$stateChangeSuccess',
              function (event, toState, toParams, fromState, fromParams) {
                  if (fromState.name === "main.cruddetail.compositiondetail") {
                      crudContextService.leavingCompositionDetail();
                  }

                  if (toState.name.startsWith("main.cruddetail")) {
                      //needs to refresh the displayables and datamap everytime the detail page is loaded.
                      init();
                      if (toState.name === "main.cruddetail.maininput") {
                          crudContextService.resetTab();
                      }
                  } else{
//                      crudContextService.leavingDetail();
//                      if (!toState.name.startsWith("main.crud")) {
//                          crudContextService.resetContext();
//                      }
                  }
              });

        init();

    }]);

})(softwrench);


