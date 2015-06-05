softwrench.controller('CrudListController', function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService, $ionicScrollDelegate) {

    $scope.moreItemsAvailable = true;
    $scope._searching = false;

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


    $scope.disableSearch = function () {
        $scope._searching = false;
        $scope.searchQuery = null;
        crudContextService.filterList(null);
        $ionicScrollDelegate.scrollTop();
    }

    $scope.itemTitle = function (item) {
        var listSchema = crudContextService.currentListSchema();
        return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "title")];
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
        $scope._searching = false;
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

    $scope.loadMore = function () {
        var log = $log.get("crudListController#loadMore");
        log.debug("fetching more items");
        crudContextService.loadMorePromise().then(function (results) {
            $scope.$broadcast('scroll.infiniteScrollComplete');
        });
    }



}
);