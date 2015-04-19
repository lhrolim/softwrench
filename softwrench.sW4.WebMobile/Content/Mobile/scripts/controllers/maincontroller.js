softwrench.controller('MainController', function ($scope, $ionicSideMenuDelegate,menuModelService) {
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

    init();



})