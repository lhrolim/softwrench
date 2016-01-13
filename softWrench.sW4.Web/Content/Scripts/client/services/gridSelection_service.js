
(function (angular) {
	"use strict";

	function gridSelectionService(crudContextHolderService) {
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

			var selectionModel = crudContextHolderService.getSelectionModel(panelid);
			selectionModel.onPageSelectedCount = 0;
			selectionModel.selectAllValue = datamaps.length > 0;
			selectionModel.pageSize = datamaps.length;

			for (var i = 0; i < datamaps.length; i++) {
			    var row = datamaps[i];
				var rowSelected = updateRowState(row, schema, panelid);
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
		    var selected = datamap.fields["_#selected"];
		    var rowId = datamap.fields[schema.idFieldName];
		    var selectionModel = crudContextHolderService.getSelectionModel(panelid);
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
         * Updates row selection state based on buffer.
         * 
         * @param {} row
         * @param {} schema 
         * @param {} panelid
		 * @returns Boolean The new selection state of the row.
         */
		function updateRowState(row, schema, panelid) {
			var buffer = crudContextHolderService.getSelectionModel(panelid).selectionBuffer;
			var rowId = row.fields[schema.idFieldName];
			var selected = Boolean(buffer[rowId]);
			row.fields["_#selected"] = selected;
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
			for (var i = 0; i < datamap.length; i++) {
				datamap[i].fields["_#selected"] = selectedValue;
				selectionChanged(datamap[i],  schema, false, panelid);
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
		    datamap.fields["_#selected"] = !datamap.fields["_#selected"];
			selectionChanged(datamap, schema, true, panelid);
		}

	    //#endregion

		//#region Service Instance
		var service = {
			gridDataChanged: gridDataChanged,
			selectionChanged: selectionChanged,
			toggleSelection: toggleSelection,
			updateRowState: updateRowState,
			selectAllChanged: selectAllChanged
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("sw_layout").factory("gridSelectionService", ["crudContextHolderService", gridSelectionService]);

	//#endregion

})(angular);