(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('checkpointService', ["contextService", "searchService", "previousFilterService", function (contextService, searchService, previousFilterService) {


            /// <summary>
            /// 
            /// </summary>
            /// <param name="schema"></param>
            /// <param name="gridData">An object combining the following properties:
            /// 
            /// searchData -->
            /// searchSort -->
            /// searchOperators -->
            /// paginationData -->
            /// 
            /// </param>
            function createGridCheckpointFromGridData(schema, gridData) {
                if (gridData == null) {
                    throw new Error('gridData should not be null');
                }

                var quickserarchDTO = null;
                if (gridData.vm) {
                    quickserarchDTO = gridData.vm.quickSearchDTO;
                }
                const searchDTO = searchService.buildSearchDTO(gridData.searchData, gridData.searchSort, gridData.searchOperator, null, gridData.paginationData, null, quickserarchDTO);
                this.createGridCheckpoint(schema, searchDTO);
                previousFilterService.createPreviousFilter(schema, gridData.searchData, gridData.searchOperator, gridData.searchSort);
            };

            function createGridCheckpoint(schema, searchDTO) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                const checkpointData = {
                    listContext: searchDTO,
                    applicationKey: applicationKey
                };
                var currentCheckpointItem = contextService.fetchFromContext('checkpointdata', true, false, false);
                if (!currentCheckpointItem) {
                    currentCheckpointItem = {};
                }
                currentCheckpointItem[applicationKey] = checkpointData;

                contextService.insertIntoContext('checkpointdata', currentCheckpointItem, false);
            };

            function fetchCheckpoint(applicationKey) {
                const checkPointArray = this.fetchAllCheckpointInfo();
                return checkPointArray.firstOrDefault(function(item) {
                    return item.applicationKey === applicationKey;
                });
            }

            function fetchAllCheckpointInfo() {
                const checkPointItem = contextService.fetchFromContext('checkpointdata', true, false, false);
                if (checkPointItem == null) {
                    return [];
                }
                const result = []; //converting to array before sending to the server
                for (let item in checkPointItem) {
                    if (!checkPointItem.hasOwnProperty(item)) {
                        continue;
                    }
                    result.push(checkPointItem[item]);
                }
                return result;
            };

            function clearCheckpoints() {
                //just removing from the context here
                contextService.deleteFromContext('checkpointdata');
            }

            const api = {
                createGridCheckpointFromGridData,
                createGridCheckpoint,
                fetchAllCheckpointInfo,
                fetchCheckpoint,
                clearCheckpoints
            };
            return api;

        }]);

})(angular);