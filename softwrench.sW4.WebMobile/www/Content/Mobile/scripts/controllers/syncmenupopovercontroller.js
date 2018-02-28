(function (softwrench) {
    "use strict";

    softwrench.controller('SyncMenuPopoverController', ["$log", "$scope", "synchronizationFacade",
        function ($log, $scope, synchronizationFacade) {


            $scope.doSync = function () {
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



