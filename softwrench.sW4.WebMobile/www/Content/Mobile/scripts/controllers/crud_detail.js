
(function (softwrench) {
    "use strict";

    softwrench.controller("CrudDetailController", ['$log', '$scope', '$rootScope', 'schemaService',
    'crudContextService', 'fieldService', 'offlineAssociationService', '$ionicPopover', '$ionicPopup', '$ionicHistory', '$ionicScrollDelegate', 'eventService', "expressionService",
    function (log, $scope, $rootScope, schemaService,
    crudContextService, fieldService, offlineAssociationService, $ionicPopover, $ionicPopup, $ionicHistory, $ionicScrollDelegate, eventService, expressionService) {

        function init() {
            $scope.displayables = crudContextService.mainDisplayables();
            $scope.schema = crudContextService.currentDetailSchema();
            $scope.datamap = crudContextService.currentDetailItemDataMap();
            eventService.onload($scope, $scope.schema, $scope.datamap, {});
        }

        $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
            scope: $scope,
        }).then(function (popover) {
            $scope.compositionpopover = popover;
        });



        $scope.expandCompositions = function ($event) {
            $scope.compositionpopover.show($event);
        }

        $scope.$on("sw_compositionselected", function () {
            $scope.compositionpopover.hide();
        });



        $scope.loadMainTab = function () {
            $ionicScrollDelegate.scrollTop();
            crudContextService.loadTab(null);
        }

        $scope.tabTitle = function () {
            return crudContextService.tabTitle();
        }


        $scope.showNavigation = function () {
            return this.isOnMainTab() && !crudContextService.hasDirtyChanges();
        }

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
                    crudContextService.cancelChanges();
                    $scope.datamap = crudContextService.currentDetailItemDataMap();
                }
            });
        }

        $scope.saveChanges = function () {
            crudContextService.saveChanges().then(function () {
                init();
            });
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

        $scope.addCompositionAllowed = function() {
            const context = crudContextService.getCrudContext();
            const composition = context.composition;
            return composition &&
                composition.currentTab &&
                composition.currentTab.schema &&
                expressionService.evaluate(composition.currentTab.schema.allowInsertion, $scope.datamap, { schema: $scope.schema }, null);
        };


        $scope.detailSummary = function () {
            return schemaService.getTitle(crudContextService.currentDetailSchema(), $scope.datamap, true);
        }

        $scope.addCompositionItem = function () {
            crudContextService.createNewCompositionItem();
        }

        $scope.navigateNext = function () {
            crudContextService.navigateNext().then(function () {
                $scope.datamap = crudContextService.currentDetailItemDataMap();
            });
        }

        $scope.navigatePrevious = function () {
            crudContextService.navigatePrevious();
            $scope.datamap = crudContextService.currentDetailItemDataMap();
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

        $rootScope.$on('sw_cruddetailrefreshed', function () {
            $scope.datamap = crudContextService.currentDetailItemDataMap();
        });

        $rootScope.$on('$stateChangeSuccess',
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
                      crudContextService.leavingDetail();
                      if (!toState.name.startsWith("main.crud")) {
                          crudContextService.resetContext();
                      }
                  }
              });

        init();

    }]);

})(softwrench);


