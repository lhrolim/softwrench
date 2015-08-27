(function (softwrench) {
    "use strict";
    
    softwrench.controller('MainController', ["$scope", "routeService", "$ionicSideMenuDelegate", "menuModelService", "crudContextService", "$ionicPopup", "securityService",
        function ($scope, routeService, $ionicSideMenuDelegate, menuModelService, crudContextService, $ionicPopup, securityService) {

        $scope.data = {};

        function init() {
            //$scope.menuleafs = ;
        }

        $scope.menuleafs = function () {
            return menuModelService.getMenuItens();
        }

        $scope.isList = function () {
            return crudContextService.isList();
        }

        $scope.toggleLeft = function () {
            $ionicSideMenuDelegate.toggleLeft();
        }


        $scope.title = function () {
            return crudContextService.currentTitle();
        }

        $scope.loadApplication = function (menuleaf) {
            crudContextService.loadApplicationGrid(menuleaf.application, menuleaf.title, menuleaf.schema);
            $ionicSideMenuDelegate.toggleLeft();
        }

        $scope.loadAction = function (action) {
            routeService.go(action);
            $ionicSideMenuDelegate.toggleLeft();
        }

        $scope.logout = function () {
            $ionicPopup.confirm({
                title: "Logout",
                template: "Are you sure you want to logout?"
            }).then(function (res) {
                if (res) {
                    securityService.logout();
                    routeService.go("login");
                }
            }).finally(function () {
                $ionicSideMenuDelegate.toggleLeft();
            });
        };

        init();


    }]);

})(softwrench);


