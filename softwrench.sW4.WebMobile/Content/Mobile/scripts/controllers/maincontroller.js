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
        crudContextService.loadApplication(menuleaf.application, menuleaf.schema);
        $ionicSideMenuDelegate.toggleLeft();
    }

    init();



})