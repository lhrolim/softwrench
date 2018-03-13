
(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("BoldItalicUnderlineController",
        ["$scope", "$rootScope", "$log", "crudContextHolderService", "checkListTableBuilderService", function ($scope, $rootScope, $log, crudContextHolderService) {


            $scope.toggle= function(btnType) {
                const dm = crudContextHolderService.rootDataMap("#modal");
                dm[btnType] = !dm[btnType];
            }

            $scope.isActive = function (btnType) {
                const dm = crudContextHolderService.rootDataMap("#modal");
                if (dm) {
                    const isActive = !!dm[btnType];
                    return isActive ? "active" : null;
                }
                
            }



        }]);


})(angular);