(function (softwrench) {
    "use strict";

    softwrench.controller('GridItemPopOverController', ["$log", "$scope", "synchronizationFacade", "crudContextService", "itemActionService", "$ionicPopup", "menuModelService",
        function ($log, $scope, synchronizationFacade, crudContextService, itemActionService, $ionicPopup, menuModelService) {


            $scope.quicksync = function () {
                const item = crudContextService.getCrudContext().currentPopOverItem;
                if (!item.isDirty) {
                    return;
                }
                return synchronizationFacade.syncItem(item).then(() => {
                    menuModelService.updateAppsCount();
                    //updating the item on the list after it has been synced
                    crudContextService.refreshGrid();
                }).then(r => {
                    $ionicPopup.alert({
                        title: "Quick Sync",
                        template: "Sync Successful"
                    });
                }).finally(() => {
                    $scope.$emit("sw_griditemoperationperformed");
                });
            }


            $scope.restorestate = function () {
                const item = crudContextService.getCrudContext().currentPopOverItem;
                return itemActionService.deleteOrRestoreItem(item)
                    .then(res => res ? crudContextService.refreshGrid() : null)
                    .finally(() => {
                        $scope.$emit("sw_griditemoperationperformed");
                    });
            };



        }]);

})(softwrench);



