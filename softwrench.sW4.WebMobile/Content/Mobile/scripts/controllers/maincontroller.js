softwrench.controller('MainController', function ($scope, menuModelService) {
    $scope.data = {};

    function init() {
        //$scope.menuleafs = ;
    }

    $scope.menuleafs = function() {
        return menuModelService.getMenuItens();
    }

    init();




    $scope.leftButtons = [{
        type: 'button-icon icon ion-navicon',
        tap: function (e) {
            $scope.sideMenuController.toggleLeft();
        }
    }];


})