(function (angular) {
    "use strict";

angular.module("sw_layout")
    .controller("RefreshController", ["$scope", "$rootScope", "crudContextHolderService", "searchService",
            function ($scope, $rootScope, crudContextHolderService, searchService) {

                $scope.refreshGrid = function () {
                    searchService.refreshGrid({}, null, { panelid: $scope.panelid, forcecleanup: true, addPreSelectedFilters: true });
                    crudContextHolderService.clearSelectionBuffer();
                    $rootScope.$broadcast(JavascriptEventConstants.GRID_REFRESH2, $scope.panelid);
                };

                $scope.shouldShowRefreshButton = function() {
                    return !crudContextHolderService.getSelectionModel($scope.panelid).showOnlySelected;
                }
            }
    ])
    .directive("refreshFilter", ["contextService", function (contextService) {
    var directive = {
        restrict: "E",
        templateUrl: contextService.getResourceUrl("/Content/Templates/commands/grid/refresh.html"),
        controller: "RefreshController"
    };
    return directive;
}
]);

})(angular);