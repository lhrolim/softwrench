(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('checkpointService', ["contextService", "searchService", function (contextService, searchService) {

    return {

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
        createGridCheckpointFromGridData: function (schema, gridData) {
            if (gridData == null) {
                throw new Error('gridData should not be null');
            }
            var searchDTO = searchService.buildSearchDTO(gridData.searchData, gridData.searchSort, gridData.searchOperator, null, gridData.paginationData);

            this.createGridCheckpoint(schema, searchDTO);

        },

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="searchDTO">the searchDTO that will be sent to the server
        /// 
        /// 
        /// </param>
        createGridCheckpoint: function (schema, searchDTO) {
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
        },

        fetchCheckpoint: function () {
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
        },

        clearCheckpoints: function () {
            //just removing from the context here
            contextService.deleteFromContext('checkpointdata');
        }
    };

}]);

})(angular);