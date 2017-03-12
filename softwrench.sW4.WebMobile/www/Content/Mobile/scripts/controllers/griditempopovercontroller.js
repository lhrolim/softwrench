(function (softwrench) {
    "use strict";

    softwrench.controller('GridItemPopOverController', ["$log", "$scope", "synchronizationFacade", "crudContextService", "itemActionService",
        function ($log, $scope, synchronizationFacade, crudContextService, itemActionService) {


            $scope.quicksync = function (item) {
                if (!item.isDirty) {
                    return;
                }
                return synchronizationFacade.syncItem(item).then(() => {
                    //updating the item on the list after it has been synced
                    crudContextService.refreshGrid();
                }).finally(() => {
                    $scope.$emit("sw_griditemoperationperformed");
                });
            }


            $scope.restorestate = function (item) {
                return itemActionService.deleteOrRestoreItem(item)
                    .then(res => res ? crudContextService.refreshGrid() : null)
                    .finally(() => {
                        $scope.$emit("sw_griditemoperationperformed");
                    });
            };



        }]);

})(softwrench);



