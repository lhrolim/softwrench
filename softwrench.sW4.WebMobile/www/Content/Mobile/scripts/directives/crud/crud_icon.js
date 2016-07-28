(function (angular) {
    "use strict";

    angular.module("softwrench").directive("crudIcon", [function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/crud/crud_icon.html",
            transclude: false,
            replace: false,
            scope: {
                item: "="
            },
            controller: ["$scope", "iconProviderService", function ($scope, iconProviderService) {

                $scope.getIconClass = item => iconProviderService.getIconClass(item);

                $scope.getIconColor = item => iconProviderService.getIconColor(item);

                $scope.getIconText = item => iconProviderService.getIconText(item);

                $scope.getIconIcon = item => iconProviderService.getIconIcon(item);

                $scope.getTextColor = item => iconProviderService.getTextColor(item);

            }]
        };

        return directive;
    }]);

})(angular);