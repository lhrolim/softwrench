(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListController", ["$log", '$scope', 'crudContextService', 'offlineSchemaService', 'statuscolorService', '$ionicScrollDelegate', '$timeout', '$ionicPopover', 'eventService', "routeConstants", "synchronizationFacade", "routeService", "crudContextHolderService", "$ionicPopup", 
        function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate, $timeout, $ionicPopover, eventService, routeConstants, synchronizationFacade, routeService, crudContextHolderService, $ionicPopup) {

            $scope.crudlist = {
                items: [],
                moreItemsAvailable: true
            };

            function initializeList() {
                $scope.crudlist.moreItemsAvailable = true;
                // getting references to elements instead of to the whole list
                $scope.crudlist.items = crudContextService.itemlist().map(i => i);
            }

            function init() {
                initializeList();
                $scope._searching = false;
                //
                //                $ionicPopover.fromTemplateUrl("Content/Mobile/templates/filteroptionsmenu.html", {
                //                    scope: $scope,
                //                }).then(popover => 
                //                    $scope.filteroptionspopover = popover
                //                );

                $timeout(() => $ionicScrollDelegate.scrollTop(), 0, false);

                const schema = crudContextService.currentListSchema();
                const datamap = crudContextService.itemlist();
                eventService.onload($scope, schema, datamap, { schemaId: schema.schemaId });

                $scope.quickSearch = crudContextHolderService.getQuickSearch();
                if (typeof $scope.quickSearch.value === "undefined") {
                    $scope.quickSearch.value = null;
                }
            }

            init();

            $scope.isSearching = function () {
                return $scope._searching;
            }

            $scope.enableSearch = function () {
                $scope._searching = true;
                $ionicScrollDelegate.scrollTop();
                // $scope.moreItemsAvailable = false;
            }

            $scope.goToAdvancedFilter = function () {
                routeService.go(".search", {});
            }

            $scope.filter = function () {
                crudContextService.refreshGrid(true).then(initializeList);
            };

            $scope.isDirty = function (item) {
                return item.isDirty;
            }

            $scope.isPending = function (item) {
                return item.pending;
            }

            $scope.hasProblem = function (item) {
                return item.hasProblem;
            }

            $scope.showFilterOptions = function ($event) {
                $scope.filteroptionspopover.show($event);
            }

            $scope.$on("sw_filteroptionclosed", function () {
                $scope.filteroptionspopover.hide();
            });

            $scope.createEnabled = function () {
                const schema = crudContextService.currentListSchema();
                if (!schema) {
                    return false;
                }
                const disabled = schema.properties["list.offline.create.disabled"];
                return disabled !== "true" && disabled !== true && crudContextService.hasNewSchemaAvailable();
            };

            $scope.disableSearch = function (clear) {
                $scope._searching = false;
                if (clear) {
                    crudContextHolderService.clearGridSearchValues();
                    crudContextService.refreshGrid();
                }
                $ionicScrollDelegate.scrollTop();
            }

            $scope.gridTitle = function () {
                const schema = crudContextHolderService.currentListSchema();
                return crudContextService.gridTitle(schema);
            }

            $scope.itemTitle = function (item) {
                const title = offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "title", item);
                if (title == null) {
                    return `New ${crudContextService.currentTitle()}`;
                }
                return title;
            }

            $scope.itemSubTitle = function (item) {
                return offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "subtitle", item);
            }

            $scope.itemFeatured = function (item) {
                return offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "featured", item);
            }

            $scope.itemExcerpt = function (item) {
                return offlineSchemaService.buildDisplayValue(crudContextService.currentListSchema(), "excerpt", item);
            }

            $scope.openDetail = function (item) {
                crudContextService.loadDetail(item);
            }

            $scope.createItem = function () {
                crudContextService.createDetail();
            }

            $scope.quickSync = function (item) {
                if (!item.isDirty) {
                    return;
                }
                synchronizationFacade.syncItem(item).then(() => {
                    //updating the item on the list after it has been synced
                    crudContextService.refreshGrid();
                });
            }
  		
            $scope.deleteOrRestoreItem = function(item) {
                const restorable = item.remoteId && item.isDirty && !!item.originaldatamap;
                const deletable = !item.remoteId;
                if (item.pending || (!restorable && !deletable)) return;

                const gridTitle = $scope.gridTitle();
                const confirmConfig = restorable
                    ? { title: "Cancel Changes", template: `Are you sure you want to cancel changes made to this ${gridTitle}` }
                    : { title: `Delete ${gridTitle}`, template: `Are you sure you want to delete this ${gridTitle} created locally` }

                return $ionicPopup.confirm(confirmConfig).then(res => {
                    if (!res) return;

                    const promise = restorable
                        ? crudContextService.restoreItemToOriginalState(item)
                        : crudContextService.deleteLocalItem(item);

                    return promise
                        .then(() => $ionicPopup.alert({ title: `${gridTitle} was successfuly ${restorable ? "restored" : "deleted"}` }))
                        .then(crudContextService.refreshGrid());
                });
            };


            $scope.$on("$stateChangeSuccess",
                 function (event, toState, toParams, fromState, fromParams) {
                     $log.get("crudlist#statehandler").debug("handler called", arguments);
                     if (!toState.name.startsWith("main.crud")) {
                         //                         crudContextService.resetContext();
                         //                         $scope.crudlist.items = [];
                         //                         $scope.crudlist.moreItemsAvailable = false;
                     } else if (!toState.name.startsWith("main.crudlist")) {
                         //to avoid strange transitions on the screen
                         //TODO: transition finished event??
                         $timeout(() => $scope.disableSearch(), 500);
                     } else {
                         init();
                     }
                 });

            $scope.$on(routeConstants.events.sameStateTransition, (event, state) => {
                $log.get("crudlist#statehandler").debug("handler called", arguments);
                if (!state.name.startsWith("main.crudlist")) return;
                init();
            });

            $scope.loadMore = function () {
                const log = $log.get("crudListController#loadMore");
                log.debug("fetching more items");
                crudContextService.loadMorePromise().then(results => {
                    $scope.$broadcast('scroll.infiniteScrollComplete');
                    $scope.crudlist.items = $scope.crudlist.items.concat(results);
                    $scope.crudlist.moreItemsAvailable = results.length > 0;
                });
            }

        }]);

})(softwrench);
