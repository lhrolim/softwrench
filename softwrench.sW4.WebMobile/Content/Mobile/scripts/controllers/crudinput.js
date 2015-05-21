softwrench.controller('CrudInputController', function ($log, $scope, crudContextService, offlineSchemaService, statuscolorService) {

    $scope.noMoreItemsAvailable = false;

    $scope.title = function () {
        return crudContextService.currentTitle();
    }

    $scope.nontabfields = function (field) {
        return crudContextService.itemlist();
    }





}
);