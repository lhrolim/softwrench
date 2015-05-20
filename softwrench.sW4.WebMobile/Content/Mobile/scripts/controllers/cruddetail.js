softwrench.controller('CrudDetailController', function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService) {

    $scope.noMoreItemsAvailable = false;

    $scope.title = function () {
        return crudContextService.currentTitle();
    }

    $scope.list = function () {
        return crudContextService.itemlist();
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
        return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "subtitle")];
    }

    $scope.itemExcerpt = function (item) {
        var listSchema = crudContextService.currentListSchema();
        return item[offlineSchemaService.locateAttributeByQualifier(listSchema, "excerpt")];
    }

    $scope.getStatusColor = function (item) {
        return statuscolorService.getColor(item["status"], crudContextService.currentApplicationName());
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