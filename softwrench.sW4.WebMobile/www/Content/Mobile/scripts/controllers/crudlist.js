(function (softwrench) {
    "use strict";

    softwrench.controller("CrudListController", ["$log", '$scope', 'crudContextService', 'offlineSchemaService', 'statuscolorService', '$ionicScrollDelegate', '$timeout', '$ionicPopover', 'eventService', "routeConstants", 
        function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate, $timeout, $ionicPopover, eventService, routeConstants) {

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
            }

            init();

            $scope.list = function () {
                return !this.isSearching() || window.nullOrEmpty($scope.searchQuery)
                    ? crudContextService.itemlist()
                    : crudContextService.getFilteredList();
            }

            $scope.isSearching = function () {
                return $scope._searching;
            }

            $scope.enableSearch = function () {
                $scope._searching = true;
                $ionicScrollDelegate.scrollTop();
                // $scope.moreItemsAvailable = false;
            }


            $scope.filter = function (data) {
                const text = data.searchQuery;
                $scope.searchQuery = text;
                crudContextService.filterList(text);
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

            $scope.disableSearch = function () {
                $scope._searching = false;
                $scope.searchQuery = null;
                crudContextService.filterList(null);
                $ionicScrollDelegate.scrollTop();
            }

            $scope.itemTitle = function (item) {
                const listSchema = crudContextService.currentListSchema();
                const title = item[offlineSchemaService.locateAttributeByQualifier(listSchema, "title")];
                if (title == null) {
                    return "New " + crudContextService.currentTitle();
                }
                return title;
            }

            $scope.itemSubTitle = function (item) {
                const listSchema = crudContextService.currentListSchema();
                return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "subtitle")];
            }

            $scope.itemFeatured = function (item) {
                const listSchema = crudContextService.currentListSchema();
                return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "featured")];
            }

            $scope.itemExcerpt = function (item) {
                const listSchema = crudContextService.currentListSchema();
                return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "excerpt")];
            }

            $scope.getStatusColor = function (item) {
                return statuscolorService.getColor(item["status"], crudContextService.currentApplicationName());
            }

            $scope.openDetail = function (item) {
                crudContextService.loadDetail(item);
            }

            $scope.createItem = function () {
                crudContextService.createDetail();
            }

            $scope.getStatusText = function (item) {
                const status = item["status"];
                return status == null ? "N" : status.charAt(0);
            }

            $scope.getTextColor = function (item) {
                const background = statuscolorService.getColor(item["status"], crudContextService.currentApplicationName());
                return background === "white" || background === "transparent" ? "black" : "white";
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