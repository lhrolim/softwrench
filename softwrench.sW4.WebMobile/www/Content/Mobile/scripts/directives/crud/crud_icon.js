(function (angular) {
    "use strict";

    angular.module("softwrench").directive("crudIcon", [function () {
        const directive = {
            restrict: "E",
            templateUrl: getResourcePath("Content/Mobile/templates/directives/crud/crud_icon.html"),
            transclude: false,
            replace: false,
            scope: {
                item: "=",
                isdetail: "=",
                iscomposition: "@",
                datamap: "=" // used on detail
            },
            controller: ["$scope", "iconProviderService", "crudContextService", "$rootScope", function ($scope, iconProviderService, crudContextService, $rootScope) {
                const createIcon = function (item) {
                    $scope.icon = iconProviderService.getIcon(item, $scope.iscomposition);
                }

                createIcon($scope.isdetail ? crudContextService.currentDetailItem() : $scope.item);

                if ($scope.isdetail) {
                    $rootScope.$on(JavascriptEventConstants.CrudSaved,() => {
                            createIcon(crudContextService.currentDetailItem());
                    });

                    $rootScope.$on("sw.labor.start", () => {
                        createIcon(crudContextService.currentDetailItem());
                    });

                    $rootScope.$on("sw.labor.stop", () => {
                        createIcon(crudContextService.currentDetailItem());
                    });

                    $rootScope.$on("sw.sync.quicksyncfinished", () => {
                        createIcon(crudContextService.currentDetailItem());
                    });

                    $rootScope.$on("sw.problem.problemupdated", () => {
                        createIcon(crudContextService.currentDetailItem());
                    });

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