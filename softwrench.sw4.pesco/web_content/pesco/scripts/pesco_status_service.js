
(function (angular) {
	"use strict";

	function pescoStatusService($log, searchService, genericTicketService, $q, crudContextHolderService, redirectService, restService, alertService) {
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

        function hasSelectedItemsForBatchStatus() {
            return Object.keys(crudContextHolderService.getSelectionModel().selectionBuffer).length > 0;
        }

        function validateBatchStatusChange(selectedItems) {
            // check if user selected at least one entry
            if (selectedItems.length <= 0) {
                alertService.alert("Please select at least one entry to proceed.");
                return false;
            }

            // check if user selected items with different status
            var differentStatus = selectedItems
                .map(function (item) {
                    return item["status"];
                })
                .distinct();
            var hasDifferentStatus = differentStatus.length > 1;

            if (hasDifferentStatus) {
                var statusForMessage = differentStatus.map(function(s) { return "'" + s + "'" }).join(", ");
                alertService.alert(
                    "You selected entries with status values of {0}.".format(statusForMessage) +
                    "<br>" +
                    "Please select entries with the same status to proceed."
                    );
                return false;
            }

            return true;
        }

        function initBatchStatusChange(schema, datamap) {
            var log = $log.get("pescoStatusService#initBatchStatus", ["batch"]);

            var application = schema.applicationName;
            var schemaId = schema.schemaId;
            
            // items selected in the buffer
            var selectedItems = Object.values(crudContextHolderService.getSelectionModel().selectionBuffer).map(function (selected) {
                return selected.fields;
            });

            // invalid selection
            if (!validateBatchStatusChange(selectedItems)) return;

            log.debug("initializing batch status change for [application: {0}, schema: {1}]".format(application, schemaId));

            redirectService.openAsModal(application, "batchStatusChangeModal", {
                savefn: function (modalData, modalSchema) {
                    var newStatus = modalData["status"];
                    
                    // only changed data + ids
                    var itemsToSubmit = selectedItems.map(function(selected) {
                        var dehydrated = { status: newStatus };
                        dehydrated[schema.idFieldName] = selected[schema.idFieldName];
                        dehydrated[schema.userIdFieldName] = selected[schema.userIdFieldName];
                        dehydrated["siteid"] = selected["siteid"];
                        dehydrated["orgid"] = selected["orgid"];
                        return dehydrated;
                    });

                    log.debug("submitting:", itemsToSubmit);

                    return restService.post("PescoBatch", "ChangeStatus", { application: application }, itemsToSubmit)
                        .then(function () {
                            log.debug("clearing selection buffer and realoading [application: {0}, schema: {1}]".format(application, schemaId));
                            crudContextHolderService.clearSelectionBuffer();
                            return redirectService.goToApplication(application, schemaId);
                        });
                }
            });
        }

		//#endregion

		//#region Service Instance
		var service = {
		    changeStatusToPending: changeStatusToPending,
		    hasSelectedItemsForBatchStatus: hasSelectedItemsForBatchStatus,
		    initBatchStatusChange: initBatchStatusChange
		};
		return service;
		//#endregion
	}

	//#region Service registration

	angular.module("pesco").clientfactory("pescoStatusService", ["$log", "searchService", "genericTicketService", "$q", "crudContextHolderService", "redirectService", "restService", "alertService", pescoStatusService]);

	//#endregion

})(angular);