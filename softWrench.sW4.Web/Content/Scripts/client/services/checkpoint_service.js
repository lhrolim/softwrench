(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('checkpointService', ["contextService", "searchService", "crudContextHolderService", "userPreferencesService", function (contextService, searchService, crudContextHolderService, userPreferencesService) {
            const userGridCheckpointKey = "userGridCheckpoint";

            function buildSearchDTO(gridData) {
                var quicksearchDTO = null;
                if (gridData.vm) {
                    quicksearchDTO = gridData.vm.quickSearchDTO;
                }

                const multiSort = crudContextHolderService.getSortModel(gridData.panelid).sortColumns;
                const searchDTO = searchService.buildSearchDTO(gridData.searchData, gridData.searchSort, gridData.searchOperator, null, gridData.paginationData, null, quicksearchDTO, multiSort);
                return new SearchDTO(searchDTO);
            }

            /**
             * 
             * @param {any} schema
             * @param {any} gridData An object combining the following properties:
            ///
            /// searchData -->
            /// searchSort -->
            /// searchOperators -->
            /// paginationData -->
             * @param {any} panelid
             */
            function createGridCheckpointFromGridData(schema, gridData, panelid) {
                if (gridData == null) {
                    throw new Error('gridData should not be null');
                }

                const dto = buildSearchDTO(gridData);
                if (!dto.isDefault()) {
                    this.createGridCheckpoint(schema, dto, panelid);
                }

                //                previousFilterService.createPreviousFilter(schema, gridData.searchData, gridData.searchOperator, gridData.searchSort);
            };

            function createGridCheckpoint(schema, searchDTO, panelid) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                const checkpointData = {
                    listContext: searchDTO,
                    applicationKey,
                    panelid
                };
                const selectedFilter = crudContextHolderService.getSelectedFilter();
                if (!!selectedFilter && selectedFilter.id !==-2) {
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

                // creates a user pref for main grids (panelid === undefined) - persists even after logout
                if (panelid === undefined) {
                    userPreferencesService.setSchemaPreference(userGridCheckpointKey, checkpointData, schema.applicationName, schema.schemaId, panelid);
                }
            };

            function fetchCheckpoint(applicationKey,panelid) {
                const checkPointArray = this.fetchAllCheckpointInfo();
                let checkpoint = checkPointArray.firstOrDefault(item => {
                    return item.applicationKey === applicationKey && item.panelid == panelid;
                });

                if (checkpoint) {
                    return checkpoint;
                }

                const tokens = applicationKey.split(".");
                return userPreferencesService.getSchemaPreference(userGridCheckpointKey, tokens[0], tokens[1]);
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