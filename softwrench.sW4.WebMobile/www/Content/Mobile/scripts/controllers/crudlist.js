(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListController", ["$log", '$scope', 'crudContextService', 'offlineSchemaService', 'statuscolorService', '$ionicScrollDelegate', '$timeout', '$ionicPopover', 'eventService', "routeConstants", "synchronizationFacade", "routeService", "crudContextHolderService", 
        function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate, $timeout, $ionicPopover, eventService, routeConstants, synchronizationFacade, routeService, crudContextHolderService) {

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

            $scope.createEnabled = function() {
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

            $scope.gridTitle = function() {
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

            $scope.getIconColor = function (datamap) {
                const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
                if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                    return statuscolorService.getColor(datamap["status"], crudContextService.currentApplicationName());
                }

                if (displayable.attribute === "wopriority") {
                    return statuscolorService.getPriorityColor(datamap[displayable.attribute]);
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
                if ($scope.isDirty(item) || $scope.isPending(item) || $scope.hasProblem(item)) {
                    return "";
                }

                const datamap = item.datamap;
                const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");

                if (!displayable || !displayable.attribute || displayable.attribute === "status") {
                    const status = datamap["status"];
                    return status == null ? "N" : status.charAt(0);
                }

                var value = datamap[displayable.attribute];

                if (displayable.attribute === "wopriority") {
                    item.icon = value ? null : "flag";
                    return value ? value.substring(0, 1) : "";
                }
                
                if (!value) {
                    return null;
                }
                value += "";
                return value.substring(0, 1);
            }

            $scope.getIconIcon = function (item) {
                if ($scope.isPending(item)) {
                    return "cloud";
                }

                if ($scope.hasProblem(item)) {
                    return "exclamation-triangle";
                }

                if ($scope.isDirty(item)) {
                    return "refresh";
                }

                const displayable = offlineSchemaService.locateDisplayableByQualifier(crudContextService.currentListSchema(), "icon");
                const value = item.datamap[displayable.attribute];
                if (displayable.attribute === "wopriority" && !value) {
                    return "flag";
                }

                return null;
            }

            $scope.getTextColor = function (datamap) {
                const background = $scope.getIconColor(datamap);
                return background === "white" || background === "transparent" ? "black" : "white";
            }

            $scope.quickSync = function (item) {
                synchronizationFacade.syncItem(item);
            }

            $scope.$root.$on("$stateChangeSuccess",
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
