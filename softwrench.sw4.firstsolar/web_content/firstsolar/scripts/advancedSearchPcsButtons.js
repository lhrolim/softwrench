(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("advancedSearchPcsButtons", function () {
        "ngInject";

        return {
            restrict: "E",
            transclude: true,
            scope: true,
            template: '<button class="btn btn-sm btn-defualt" ng-click="add()"><i class="fa fa-plus"></i>&ensp;ADD</button>' +
                ' <button class="btn btn-sm btn-defualt" ng-click="remove()"><i class="fa fa-minus"></i>&ensp;REMOVE</button>' +
                ' <button class="btn btn-sm btn-defualt" ng-click="clear()"><i class="fa fa-eraser"></i>&ensp;CLEAR</button>',
            controller: ["$scope", "advancedSearchService", "crudContextHolderService", function ($scope, advancedSearchService, crudContextHolderService) {

                function crudSearchDatamap() {
                    return crudContextHolderService.rootDataMap("search");
                }

                $scope.add = function() {
                    advancedSearchService.addPcsLocations(crudSearchDatamap());
                }

                $scope.remove = function () {
                    advancedSearchService.removePcsLocations(crudSearchDatamap());
                }

                $scope.clear = function () {
                    advancedSearchService.clearPcsLocations(crudSearchDatamap());
                }
            }]
        }
    });

})(angular);