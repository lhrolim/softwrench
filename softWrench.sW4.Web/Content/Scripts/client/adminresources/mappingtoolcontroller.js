
(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("MappingSpreadsheetController", ["$scope", "restService", "$log", function ($scope, restService, $log) {


        $scope.vm = {
            helpExpanded :false
        };

        const dm = $scope.datamap;

        $scope.toggleHelp = () => {
            $scope.vm.helpExpanded = !$scope.vm.helpExpanded;
        }


        const loadData = (serverResult) => {
            serverResult.data.forEach(i => {
                $scope.vm.gridoptions.data.push(i);
            });

            for (let i = 0; i < 100; i++) {
                $scope.vm.gridoptions.data.push({ "originValue": null, "destinationValue": null });
            }
        }

        $scope.$on("mappingtool.save", () => {
            var grid = $scope.gridApi.grid;
            const rowsToSave = grid.rows.map(r => r.entity).filter(r => (r.originValue != null && r.destinationValue != null));
            restService.postPromise("MappingTool", "Save", { "mappingDefinitionId": dm.id,"mappingDefinitionKey" : dm["key_"] }, rowsToSave).then(r => {
                grid.cellNav.focusedCells = [];
                $scope.$broadcast("cellNav");
                $scope.vm.gridoptions.data = [];
                loadData(r);
            });
        });

        $scope.vm.gridoptions = {
            enableCellEditOnFocus: true,
            enableFiltering: true,
            enableSorting: false,

            //paginationPageSizes: [10, 30, 100],
            //paginationPageSize: 10,
        }

        const sourceDisplayName = !!dm.sourcecolumnalias ? dm.sourcecolumnalias : "Origin Value";
        const destDisplayName = !!dm.destinationcolumnalias ? dm.destinationcolumnalias : "Destination Value";

     


        $scope.vm.gridoptions.columnDefs =
         [
             { name: 'id', visible: false },
             { name: 'originValue', displayName: sourceDisplayName, enableHiding: false },
             { name: 'destinationValue', displayName: destDisplayName, enableHiding: false },

         ];


        $scope.vm.gridoptions.onRegisterApi = function (gridApi) {
            $scope.gridApi = gridApi;
        };


        if (dm.id == null) {
            for (let i = 0; i < 100; i++) {
                $scope.vm.gridoptions.data.push({ "originValue": null, "destinationValue": null });
            }
            return;
        }


        restService.getPromise("MappingTool", "LoadMappings", { mappingDefinitionId: dm.id }).then(r => {
            loadData(r);


        });


    }]);


})(angular);