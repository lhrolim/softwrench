(function (angular) {
	"use strict";

	function multisortService(crudContextHolderService) {
		//#region Utils
	    const sortModel = (panelid) => crudContextHolderService.getSortModel(panelid);
		//#endregion

		//#region Public methods
	    function hasMultisort(panelid) {
	        const sortColumns = sortModel(panelid).sortColumns;
	        return sortColumns && sortColumns.length > 0;
	    }

	    function multsortString(panelid) {
	        const sortColumns = sortModel(panelid).sortColumns;
	        if (!sortColumns || sortColumns.length === 0) {
	            return null;
	        }

	        let sortString = "";
	        angular.forEach(sortColumns, (sortColumn) => {
	            sortString += sortColumn.columnName + " " + (sortColumn.isAscending ? "asc" : "desc") + ", ";
	        });

	        sortString = sortString.substring(0, sortString.length - 2);
	        return sortString;
	    }

	    function toMultisortColumns(multisortString) {
	        const columns = [];
	        if (!multisortString || !multisortString.trim()) {
	            return columns;
	        }

	        const entries = multisortString.split(",");
	        angular.forEach(entries, (entry) => {
	            entry = entry.trim();
	            const indexOfDot = entry.indexOf(".");
	            const end = entry.indexOf(" ") === -1 ? entry.length : entry.indexOf(" ");
	            const columnName = (indexOfDot === -1) ? entry.substring(0, end) : entry.substring(indexOfDot + 1, end);
	            const isAscending = entry.endsWith("desc") ? false : true;
	            columns.push({
	                columnName: columnName,
	                isAscending: isAscending
	            });
	        });

	        return columns;
	    }

		//#endregion

		//#region Service Instance
		const service = {
		    hasMultisort,
		    multsortString,
		    toMultisortColumns
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("sw_layout").service("multisortService", ["crudContextHolderService", multisortService]);

	//#endregion

})(angular);