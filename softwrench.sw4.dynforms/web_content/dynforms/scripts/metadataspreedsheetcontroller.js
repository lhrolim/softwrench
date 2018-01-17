
(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("MetadataOptionsController", ["$scope", "$rootScope", "restService", "applicationService", "$log", "crudContextHolderService", function ($scope, $rootScope, restService, applicationService, $log, crudContextHolderService) {


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
                //             { name: 'id', visible: false },
                { name: 'value', displayName: 'Value (To be stored on dataBase)', enableHiding: false },
                { name: 'label', displayName: 'Label (To be displayed on screen)', enableHiding: false },
            ];


        $scope.vm.gridoptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
        };

        const loadData = () => {

            const dm = crudContextHolderService.rootDataMap();
            const jsonSt = dm["#jsonList"];

            if (jsonSt) {
                const jsonArray = JSON.parse(jsonSt);
                if (jsonArray)
                    jsonArray.forEach(i => {
                        $scope.vm.gridoptions.data.push(i);
                    });
              
            }
            for (let i = 0; i < 100; i++) {
                $scope.vm.gridoptions.data.push({ "value": null, "label": null });
            }


        }


        function restoreData() {
            const grid = $scope.gridApi.grid;
            $scope.vm.gridoptions.data = [];
            grid.cellNav.focusedCells = [];
            $scope.$broadcast("cellNav");
            loadData();
        }

        $rootScope.$on("sw.crud.body.crawlocurred", () => {
            restoreData();
        });

        $rootScope.$on(JavascriptEventConstants.REDIRECT_AFTER, () => {
            restoreData();
        });

        $rootScope.$on(JavascriptEventConstants.ApplicationRedirected, () => {
            restoreData();
        });

        loadData();

        $scope.$on("metadataoptionsspreadsheet.save", () => {
            const dm = crudContextHolderService.rootDataMap();

            var grid = $scope.gridApi.grid;
            const rowsToSave = grid.rows.map(r => r.entity).filter(r => (r.value != null && r.label != null));

            dm["#jsonList"] = JSON.stringify(rowsToSave);

            return applicationService.save().then(r => {
                grid.cellNav.focusedCells = [];
                $scope.$broadcast("cellNav");
            });

        });









    }]);


})(angular);