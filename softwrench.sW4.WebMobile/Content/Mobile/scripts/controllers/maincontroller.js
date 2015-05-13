softwrench.controller('MainController', function ($scope,$state, $ionicSideMenuDelegate,menuModelService,crudContextService) {
    $scope.data = {};

    function init() {
        //$scope.menuleafs = ;
    }

    $scope.menuleafs = function() {
        return menuModelService.getMenuItens();
    }

    $scope.toggleLeft = function() {
        $ionicSideMenuDelegate.toggleLeft();
    }

    $scope.loadApplication = function (menuleaf) {
        crudContextService.loadApplicationGrid(menuleaf.application,menuleaf.title, menuleaf.schema);
        $ionicSideMenuDelegate.toggleLeft();
    }

    init();



})