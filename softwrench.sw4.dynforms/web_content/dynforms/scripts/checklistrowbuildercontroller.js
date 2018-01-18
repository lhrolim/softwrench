
(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("CheckListRowBuilderController",
        ["$scope", "$rootScope", "$log", "crudContextHolderService", "checkListTableBuilderService", function ($scope, $rootScope, $log, crudContextHolderService, checkListTableBuilderService) {


            $scope.vm = {
            };


            $scope.vm.gridoptions = {
                enableCellEditOnFocus: true,
                enableFiltering: true,
                enableSorting: false,
                data: []
            }


            $scope.vm.gridoptions.columnDefs =
                [
                    { name: 'label', displayName: 'Item', enableHiding: false },
                ];


            $scope.vm.gridoptions.onRegisterApi = function (gridApi) {
                $scope.gridApi = gridApi;
            };

            const loadData = () => {

                $scope.vm.gridoptions.data = []

                const dm = crudContextHolderService.rootDataMap("#modal");
                if (!dm) {
                    return;
                }
                const checklistrows = dm["#checklistrows"];

                if (checklistrows) {

                    checklistrows.forEach(i => {
                        $scope.vm.gridoptions.data.push({ "label": i });
                    });

                }
                for (let i = 0; i < 100; i++) {
                    $scope.vm.gridoptions.data.push({ "label": null });
                }


            }


            function restoreData() {
                const grid = $scope.gridApi.grid;
                $scope.vm.gridoptions.data = [];
                grid.cellNav.focusedCells = [];
                $scope.$broadcast("cellNav");
                loadData();
            }


            $rootScope.$on(JavascriptEventConstants.ModalClosed, () => {
                restoreData();
            });


            loadData();

            $scope.$on("dynform.checklist.onsavemodal", (event, modalData,tableMetadata) => {
                const dm = crudContextHolderService.rootDataMap("#modal");

                var grid = $scope.gridApi.grid;
                const rowsToSave = grid.rows.map(r => r.entity).filter(r => (r.label != null)).map(i=> i.label);
                dm["#checklistrows"] = JSON.stringify(rowsToSave);

                checkListTableBuilderService.convertArraysIntoRows(modalData,tableMetadata,rowsToSave);


            });

            $scope.$on("dynform.checklist.loaddata", (event, tableMetadata) => {
                loadData();
            });









        }]);


})(angular);