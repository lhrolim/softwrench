(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('checkpointService', ["contextService", "searchService", function (contextService, searchService) {


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
                var searchDTO = searchService.buildSearchDTO(gridData.searchData, gridData.searchSort, gridData.searchOperator, null, gridData.paginationData);

                this.createGridCheckpoint(schema, searchDTO);

            };

            function createGridCheckpoint(schema, searchDTO) {
                var applicationKey = schema.applicationName + "." + schema.schemaId;

                var checkpointData = {
                    listContext: searchDTO,
                    applicationKey: applicationKey
                }

                var currentCheckpointItem = contextService.fetchFromContext('checkpointdata', true, false, false);
                if (!currentCheckpointItem) {
                    currentCheckpointItem = {};
                }
                currentCheckpointItem[applicationKey] = checkpointData;

                contextService.insertIntoContext('checkpointdata', currentCheckpointItem, false);
            };

            function fetchCheckpoint(applicationKey) {
                var checkPointArray = this.fetchAllCheckpointInfo();
                return checkPointArray.firstOrDefault(function(item) {
                    return item.applicationKey === applicationKey;
                });
            }

            function fetchAllCheckpointInfo() {
                var checkPointItem = contextService.fetchFromContext('checkpointdata', true, false, false);
                if (checkPointItem == null) {
                    return [];
                }
                var result = [];
                //converting to array before sending to the server
                for (var item in checkPointItem) {
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


            var api = {
                createGridCheckpointFromGridData: createGridCheckpointFromGridData,
                createGridCheckpoint: createGridCheckpoint,
                fetchAllCheckpointInfo: fetchAllCheckpointInfo,
                fetchCheckpoint: fetchCheckpoint,
                clearCheckpoints: clearCheckpoints
            }

            return api;

        }]);

})(angular);