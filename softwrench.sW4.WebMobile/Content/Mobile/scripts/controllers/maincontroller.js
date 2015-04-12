softwrench.controller('MainController', function ($scope, swdbDAO) {
    $scope.data = {};

    function init() {
        swdbDAO.findUnique("Menu").success(function (dbMenu) {
            if (!dbMenu) {
                //instantiate a new menu on the database, with no data.
                //this will get populated upon first synchronization
                var ob = entities.Menu;
                var menu = new ob();
                swdbDAO.save(menu).success(function () {
                    $scope.menu = menu;
                });
            } else {
                $scope.menu = dbMenu[0];
            }

        });
    }

    init();




    $scope.leftButtons = [{
        type: 'button-icon icon ion-navicon',
        tap: function (e) {
            $scope.sideMenuController.toggleLeft();
        }
    }];


})