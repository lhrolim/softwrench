
(function (angular) {
	"use strict";

	function pescoStatusService($log, searchService, genericTicketService, $q) {
		//#region Utils

		//#endregion

		//#region Public methods

		function changeStatusToPending(datamap, column, schema, panelid) {
			if (column.attribute !== "pending" || datamap["pending"] !== "NEW") {
				return true;
			}
			return genericTicketService.changeStatus(datamap, schema.schemaId, "PENDING").then(function() {
			    searchService.refreshGrid(null, null, { panelid: panelid, keepfilterparams :true});
			    return $q.reject();
			});
		}

		//#endregion

		//#region Service Instance
		const service = {
		    changeStatusToPending: changeStatusToPending,
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("pesco").clientfactory("pescoStatusService", ["$log", "searchService", "genericTicketService", "$q", "crudContextHolderService", "redirectService", "restService", "alertService", pescoStatusService]);

	//#endregion

})(angular);