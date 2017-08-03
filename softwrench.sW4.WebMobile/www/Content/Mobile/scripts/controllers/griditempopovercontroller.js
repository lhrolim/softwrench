(function (softwrench) {
    "use strict";

    softwrench.controller('GridItemPopOverController', ["$log", "$scope", "synchronizationFacade", "crudContextService", "itemActionService", "$ionicPopup", "menuModelService", "alertService", "routeService", 
        function ($log, $scope, synchronizationFacade, crudContextService, itemActionService, $ionicPopup, menuModelService, alertService, routeService) {


            $scope.quicksync = function () {
                const item = crudContextService.getCrudContext().currentPopOverItem;
                if (!item.isDirty) {
                    return;
                }
                return synchronizationFacade.syncItem(item).then(() => {

                    // was called from a composition tab on detail
                    // resets state first
                    if (!crudContextService.isOnMainTab()) {
                        crudContextService.resetTab();
                        routeService.go("main.cruddetail.maininput");
                    }

                    menuModelService.updateAppsCount();
                    //updating the item on the list after it has been synced
                    crudContextService.refreshGrid();
                }).then(r => {
                    $ionicPopup.alert({
                        title: "Quick Sync",
                        template: alertService.buildCenteredTemplate("Sync Successful")
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



