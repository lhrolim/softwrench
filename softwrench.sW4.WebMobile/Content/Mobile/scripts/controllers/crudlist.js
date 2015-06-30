softwrench.controller('CrudListController', function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate, $rootScope, $timeout, $ionicPopover,schemaService) {

    'use strict';

    $scope.moreItemsAvailable = true;
    $scope._searching = false;

    $ionicPopover.fromTemplateUrl('Content/Mobile/templates/filteroptionsmenu.html', {
        scope: $scope,
    }).then(function (popover) {
        $scope.filteroptionspopover = popover;
    });

    $scope.list = function () {
        if (!this.isSearching() || nullOrEmpty($scope.searchQuery)) {
            return crudContextService.itemlist();
        }
        return crudContextService.getFilteredList();
    }

    $scope.isSearching = function () {
        return $scope._searching;
    }

    $scope.enableSearch = function () {
        $scope._searching = true;
        $ionicScrollDelegate.scrollTop();
        //        $scope.moreItemsAvailable = false;
    }


    $scope.filter = function (data) {
        var text = data.searchQuery;
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

    $scope.disableSearch = function () {
        $scope._searching = false;
        $scope.searchQuery = null;
        crudContextService.filterList(null);
        $ionicScrollDelegate.scrollTop();
    }

    $scope.itemTitle = function (item) {
        var listSchema = crudContextService.currentListSchema();
        var title = item[offlineSchemaService.locateAttributeByQualifier(listSchema, "title")];
        if (title == null) {
            return schemaService.getTitle(listSchema, item, true);
        }
        return title;
    }

    $scope.itemSubTitle = function (item) {
        var listSchema = crudContextService.currentListSchema();
        return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "subtitle")];
    }

    $scope.itemFeatured = function (item) {
        var listSchema = crudContextService.currentListSchema();
        return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "featured")];
    }

    $scope.itemExcerpt = function (item) {
        var listSchema = crudContextService.currentListSchema();
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
        return item["status"].charAt(0);
    }

    $scope.getTextColor = function (item) {
        var background = statuscolorService.getColor(item["status"], crudContextService.currentApplicationName());
        if (background == "white" || background == "transparent") {
            return "black";
        }
        return "white";
    }


    $rootScope.$on('$stateChangeSuccess',
         function (event, toState, toParams, fromState, fromParams) {
             if (!toState.name.startsWith("main.crudlist")) {
                 $timeout(function () {
                     //to avoid strange transitions on the screen
                     //TODO: transition finished event??
                     $scope.disableSearch();
                 }, 500);

             }
         });

    $scope.loadMore = function () {
        var log = $log.get("crudListController#loadMore");
        log.debug("fetching more items");
        crudContextService.loadMorePromise().then(function (results) {
            $scope.$broadcast('scroll.infiniteScrollComplete');
        });
    }



}
);