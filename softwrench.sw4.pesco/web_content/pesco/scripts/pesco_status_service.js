
(function (angular) {
	"use strict";

	function pescoStatusService(searchService, genericTicketService, $q) {
		//#region Utils

		//#endregion

		//#region Public methods

		function changeStatusToPending(datamap, column, schema, panelid) {
			if (column.attribute !== "pending" || datamap["pending"] !== "NEW") {
				return true;
			}
			return genericTicketService.changeStatus(datamap, schema.schemaId, "PENDING").then(function() {
			    searchService.refreshGrid(null, { panelid: panelid});
			    return $q.reject();
			});
		}

		//#endregion

		//#region Service Instance
		var service = {
			changeStatusToPending: changeStatusToPending
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("pesco").clientfactory("pescoStatusService", ["searchService", "genericTicketService", "$q", pescoStatusService]);

	//#endregion

})(angular);