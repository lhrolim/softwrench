softwrench.controller('MainController', function ($scope, routeService, $ionicSideMenuDelegate, menuModelService, crudContextService) {
    $scope.data = {};

    function init() {
        //$scope.menuleafs = ;
    }

    $scope.menuleafs = function() {
        return menuModelService.getMenuItens();
    }

    $scope.isList = function () {
        return crudContextService.isList();
    }

    $scope.toggleLeft = function() {
        $ionicSideMenuDelegate.toggleLeft();
    }

    $scope.loadApplication = function (menuleaf) {
        crudContextService.loadApplicationGrid(menuleaf.application,menuleaf.title, menuleaf.schema);
        $ionicSideMenuDelegate.toggleLeft();
    }

    $scope.loadAction = function (action) {
        routeService.go(action);
        $ionicSideMenuDelegate.toggleLeft();
    }

    init();



})