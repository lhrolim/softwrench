(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListController", ["$log", '$scope', 'crudContextService', 'offlineSchemaService', 'statuscolorService', '$ionicScrollDelegate', '$timeout', '$ionicPopover', 'eventService', "routeConstants", "synchronizationFacade", "routeService", "crudContextHolderService", 
        function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate, $timeout, $ionicPopover, eventService, routeConstants, synchronizationFacade, routeService, crudContextHolderService) {

            function init() {
                $scope.moreItemsAvailable = true;
                $scope._searching = false;

                $ionicPopover.fromTemplateUrl("Content/Mobile/templates/filteroptionsmenu.html", {
                    scope: $scope,
                }).then(popover => 
                    $scope.filteroptionspopover = popover
                );

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

            $scope.list = function () {
                return crudContextService.itemlist();
            }

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
                crudContextService.refreshGrid(true);
            };

            $scope.isDirty = function (item) {
                return item.isDirty;
            }

            $scope.isPending = function (item) {
                return item.pending;
            }

            $scope.showFilterOptions = function ($event) {
                $scope.filteroptionspopover.show($event);
            }

            $scope.$on("sw_filteroptionclosed", function () {
                $scope.filteroptionspopover.hide();
            });

            $scope.hasNewSchemaAvailable = function () {
                return crudContextService.hasNewSchemaAvailable();
            }

            $scope.disableSearch = function (clear) {
                $scope._searching = false;
                if (clear) {
                    crudContextHolderService.clearGridSearchValues();
                    crudContextService.refreshGrid();
                }
                $ionicScrollDelegate.scrollTop();
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

            $scope.getIconColor = function (item) {
                const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
                if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                    return statuscolorService.getColor(item["status"], crudContextService.currentApplicationName());
                }

                if (displayable.attribute === "wopriority") {
                    return statuscolorService.getPriorityColor(item[displayable.attribute]);
                }

                return "#777";
            }

            $scope.openDetail = function (item) {
                crudContextService.loadDetail(item);
            }

            $scope.createItem = function () {
                crudContextService.createDetail();
            }

            $scope.getIconText = function (item) {
                const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");

                if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                    const status = item["status"];
                    return status == null ? "N" : status.charAt(0);
                }

                if (displayable.attribute === "wopriority") {
                    return "\u2691";
                }

                var value = item[displayable.attribute];
                if (!value) {
                    return null;
                }
                value += "";
                return value.substring(0, 1);
            }

            $scope.getTextColor = function (item) {
                const background = $scope.getIconColor(item);
                return background === "white" || background === "transparent" ? "black" : "white";
            }

            $scope.quickSync = function (item) {
                synchronizationFacade.syncItem(item);
            }

            $scope.$root.$on("$stateChangeSuccess",
                 function (event, toState, toParams, fromState, fromParams) {
                     $log.get("crudlist#statehandler").debug("handler called", arguments);
                     if (!toState.name.startsWith("main.crud")) {
                         crudContextService.resetContext();
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
                    $scope.moreItemsAvailable = results.length > 0;
                });
            }

        }]);

})(softwrench);
