(function (angular) {
    "use strict";

    angular.module("softwrench").directive("crudIcon", [function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/crud/crud_icon.html",
            transclude: false,
            replace: false,
            scope: {
                item: "=",
                isdetail:'='
            },
            controller: ["$scope", "iconProviderService", "crudContextService", function ($scope, iconProviderService, crudContextService) {

                $scope.currentItem = function (item) {
                    return $scope.isdetail ? crudContextService.currentDetailItem() : item;
                }


//
                $scope.getIconColor = item => iconProviderService.getIconColor($scope.currentItem(item));

                $scope.getIconText = item => iconProviderService.getIconText($scope.currentItem(item));

                $scope.getIconIcon = item => iconProviderService.getIconIcon($scope.currentItem(item));

                $scope.getTextColor = item => iconProviderService.getTextColor($scope.currentItem(item));


                //                $scope.getIconClass = item => { iconProviderService.getIconClass($scope.currentItem(item)); }

                $scope.getIconClass = item => iconProviderService.getIconClass($scope.currentItem(item));

//                $scope.getIconColor = item => iconProviderService.getIconColor(item);
//
//                $scope.getIconText = item => iconProviderService.getIconText(item);
//
//                $scope.getIconIcon = item => iconProviderService.getIconIcon(item);
//
//                $scope.getTextColor = item => iconProviderService.getTextColor(item);


            }]
        };

        return directive;
    }]);

})(angular);