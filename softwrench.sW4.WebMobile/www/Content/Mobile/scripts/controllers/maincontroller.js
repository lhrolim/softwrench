(function (softwrench) {
    "use strict";

    softwrench.controller('MainController', ["$scope", "$ionicSideMenuDelegate", "crudContextService",
        function ($scope, $ionicSideMenuDelegate, crudContextService) {

            $scope.data = {};
           
            function init() {
                if (isRippleEmulator()) {
                    $("body").addClass("ripple");
                }
            }

            $scope.isList = function () {
                return crudContextService.isList();
            };

            $scope.toggleLeft = function () {
                $ionicSideMenuDelegate.toggleLeft();
            };

            $scope.closeMenu= function () {
                $ionicSideMenuDelegate.toggleLeft();
            };

            $scope.title = function () {
                return crudContextService.currentTitle();
            };
            

            init();


        }]);

})(softwrench);


