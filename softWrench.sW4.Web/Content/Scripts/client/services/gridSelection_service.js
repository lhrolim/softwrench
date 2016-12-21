
(function (angular) {
    "use strict";

    function gridSelectionService(crudContextHolderService, dispatcherService) {
        //#region Utils

        //#endregion

        //#region Public methods

        /**
		* Starts the selection buffer and select all states.
		* 
		* @param {} datamap the list of all datamaps 
		* @param {} schema 
		* @param {} panelid
		*/
        function gridDataChanged(datamaps, schema, panelid) {
            if (schema.properties["list.selectionstyle"] !== "multiple") {
                return;
            }

            var loadServerSelection = schema.properties["list.loadwithselection"];

            var selectionModel = crudContextHolderService.getSelectionModel(panelid);
            selectionModel.onPageSelectedCount = 0;
            selectionModel.selectAllValue = datamaps.length > 0;
            selectionModel.pageSize = datamaps.length;

            for (var i = 0; i < datamaps.length; i++) {
                var row = datamaps[i];
                var rowSelected = updateRowState(row, schema, panelid, loadServerSelection);
                if (rowSelected) {
                    selectionModel.onPageSelectedCount++;
                }
                selectionModel.selectAllValue = rowSelected && selectionModel.selectAllValue;
            }
        }



        /**
         * Updates the selection buffer and changes select all if needed.
         * 
         * @param {} row The row changed.
         * @param {} datamap
         * @param {} schema 
         * @param {} updatesSelectAll Whether or not updates select all value
         * @param {} panelid
         */
        function selectionChanged(datamap, schema, updatesSelectAll, panelid) {
            var selected = datamap["_#selected"];
            var selectionModel = crudContextHolderService.getSelectionModel(panelid);
            var rowId = datamap[selectionModel.selectionBufferIdCollumn];
            if (selected) {
                crudContextHolderService.addSelectionToBuffer(rowId, datamap, panelid);
                if (selectionModel.onPageSelectedCount < selectionModel.pageSize) {
                    selectionModel.onPageSelectedCount++;
                }
                if (updatesSelectAll && selectionModel.onPageSelectedCount === selectionModel.pageSize) {
                    selectionModel.selectAllValue = true;
                }
            } else {
                if (selectionModel.onPageSelectedCount > 0) {
                    selectionModel.onPageSelectedCount--;
                }
                selectionModel.selectAllValue = false;
                crudContextHolderService.removeSelectionFromBuffer(rowId, panelid);
            }
        }

        /**
		 * Function called to pre-populate the buffer based on the selection of items
		 * @param {type} datamaps
		 * @param {type} panelid
		 */
        function loadBufferFromDatamap(datamaps, schema, panelid) {
            datamaps.forEach(function (datamap) {
                selectionChanged(datamap, schema, true, panelid);
            });

        }

        /**
         * Updates row selection state based on buffer.
         * 
         * @param {} row
         * @param {} schema 
         * @param {} panelid
         * @param loadServerSelection --> if true it means that we should load the status from the datamap into the buffer. If the buffer is selected the result will remain selected though
		 * @returns Boolean The new selection state of the row.
         */
        function updateRowState(row, schema, panelid, loadServerSelection) {
            var selectionModel = crudContextHolderService.getSelectionModel(panelid);
            var buffer = selectionModel.selectionBuffer;
            var rowId = row[selectionModel.selectionBufferIdCollumn];

            if (loadServerSelection) {
                var serverValue = Boolean(row["_#selected"]);
                if (serverValue) {
                    buffer[rowId] = true;
                }
            }
            var selected = Boolean(buffer[rowId]);
            row["_#selected"] = selected;
            if (selected) {
                buffer[rowId] = row;
            }

            return selected;
        }

        /**
         * Updates the state of all row checkboxes on page based on select all current value.
         * 
         * @param {} datamap
         * @param {} schema 
         * @param {} panelid
         */
        function selectAllChanged(datamap, schema, panelid) {
            var selectedValue = crudContextHolderService.getSelectionModel(panelid).selectAllValue;
            var datamapToUse = datamap || crudContextHolderService.rootDataMap(panelid);
            var schemaToUse = schema || crudContextHolderService.currentSchema(panelid);
            for (var i = 0; i < datamapToUse.length; i++) {
                datamapToUse[i]["_#selected"] = selectedValue;
                selectionChanged(datamapToUse[i], schemaToUse, false, panelid);
            }
        }

        /**
         * Toggles row selection, updates the selection buffer and changes select all if needed.
         * 
         * @param {} row The row to toggle.
         * @param {} datamap
         * @param {} schema 
         * @param {} panelid
         */
        function toggleSelection(datamap, schema, panelid) {
            datamap["_#selected"] = !datamap["_#selected"];
            selectionChanged(datamap, schema, true, panelid);
        }

        /**
         * Clear all selected, the buffer and only selected state.
         * 
         * @param {} datamap
         * @param {} schema 
         * @param {} panelid
         */
        function clearSelection(datamap, schema, panelid) {
            var selectionModel = crudContextHolderService.getSelectionModel(panelid);
            selectionModel.selectAllValue = false;
            crudContextHolderService.clearSelectionBuffer(panelid);
            selectAllChanged(datamap, schema, panelid);
            if (selectionModel.showOnlySelected) {
                dispatcherService.dispatchevent(JavascriptEventConstants.ToggleSelected, panelid);
            }
        }

        //#endregion

        //#region Service Instance
        var service = {
            gridDataChanged: gridDataChanged,
            loadBufferFromDatamap: loadBufferFromDatamap,
            selectAllChanged: selectAllChanged,
            selectionChanged: selectionChanged,
            toggleSelection: toggleSelection,
            updateRowState: updateRowState,
            clearSelection: clearSelection
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").service("gridSelectionService", ["crudContextHolderService", "dispatcherService", gridSelectionService]);

    //#endregion

})(angular);