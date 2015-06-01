softwrench.controller('CompositionMenuController', function ($log, $scope, crudContextService) {

    $scope.compositionMenus = function () {
        return crudContextService.currentCompositionsToShow();
    }

    $scope.getTabIcon = function (tab) {
        return tab.schema.schemas.list.properties['icon.composition.tab'];
    };





}
);