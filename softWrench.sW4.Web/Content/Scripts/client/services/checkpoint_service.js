(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('checkpointService', ["contextService", "searchService", "crudContextHolderService", function (contextService, searchService, crudContextHolderService) {


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

                var quicksearchDTO = null;
                if (gridData.vm) {
                    quicksearchDTO = gridData.vm.quickSearchDTO;
                }

                const multiSort = crudContextHolderService.getSortModel(gridData.panelid).sortColumns;
                const searchDTO = searchService.buildSearchDTO(gridData.searchData, gridData.searchSort, gridData.searchOperator, null, gridData.paginationData, null, quicksearchDTO, multiSort);
                const dto = new SearchDTO(searchDTO);
                if (!dto.isDefault()) {
                    this.createGridCheckpoint(schema, dto);
                }
                
//                previousFilterService.createPreviousFilter(schema, gridData.searchData, gridData.searchOperator, gridData.searchSort);
            };

            function createGridCheckpoint(schema, searchDTO) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                const checkpointData = {
                    listContext: searchDTO,
                    applicationKey
                };
                const selectedFilter = crudContextHolderService.getSelectedFilter();
                if (!!selectedFilter) {
                    //there´s no point to create a "previous unsaved filter" if a filter has already been applied. 
                    //Hence setting this flag.
                    checkpointData["ignorepreviousfilter"] = true;
                }

                var currentCheckpointItem = contextService.fetchFromContext('checkpointdata', true, false, false);
                if (!currentCheckpointItem) {
                    currentCheckpointItem = {};
                }
                currentCheckpointItem[applicationKey] = checkpointData;

                contextService.insertIntoContext('checkpointdata', currentCheckpointItem, false);
            };

            function fetchCheckpoint(applicationKey) {
                const checkPointArray = this.fetchAllCheckpointInfo();
                return checkPointArray.firstOrDefault(item => {
                    return item.applicationKey === applicationKey;
                });
            }

        
            function getCheckPointAsFilter(applicationName,schemaId) {
                const checkPoint = this.fetchCheckpoint(applicationName + "." + schemaId);
                if (!checkPoint || checkPoint["ignorepreviousfilter"]) {
                    return null;
                }


             

                if (!(checkPoint.listContext instanceof SearchDTO)) {
                    checkPoint.listContext = new SearchDTO(checkPoint.listContext);
                }

                return GridFilterDTO.PreviousDefaultFilter(applicationName, checkPoint.listContext);
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
                getCheckPointAsFilter,
                fetchAllCheckpointInfo,
                fetchCheckpoint,
                clearCheckpoints
            };
            return api;

        }]);

})(angular);