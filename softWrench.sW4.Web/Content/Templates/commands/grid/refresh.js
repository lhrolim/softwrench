(function (angular) {
    "use strict";

angular.module("sw_layout")
.controller("RefreshController", ["$scope", "$rootScope", "crudContextHolderService", "searchService",
        function ($scope, $rootScope, crudContextHolderService, searchService) {

            $scope.refreshGrid = function () {
                searchService.refreshGrid({}, null, { panelid: $scope.panelid, forcecleanup: true, addPreSelectedFilters: true });
                $rootScope.$broadcast("sw.grid.refresh", $scope.panelid);
            };

            $scope.shouldShowRefreshButton = function() {
                return !crudContextHolderService.getSelectionModel($scope.panelid).showOnlySelected;
            }
        }
]);

})(angular);