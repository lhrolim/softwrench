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
                isdetail: "=",
                datamap: "=" // used on detail
            },
            controller: ["$scope", "iconProviderService", "crudContextService", function ($scope, iconProviderService, crudContextService) {
                const createIcon = function (item) {
                    $scope.icon = iconProviderService.getIcon(item);
                }

                createIcon($scope.isdetail ? crudContextService.currentDetailItem() : $scope.item);

                if ($scope.isdetail) {
                    $scope.$watch("datamap.approwstamp", (newRowstamp, oldRowstamp) => {
                        if (oldRowstamp && newRowstamp !== oldRowstamp) {
                            createIcon(crudContextService.currentDetailItem());
                        }
                    }, true);
                }
            }]
        };

        return directive;
    }]);

})(angular);