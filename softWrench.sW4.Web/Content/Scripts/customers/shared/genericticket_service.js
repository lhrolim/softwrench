(function (angular) {
    "use strict";

    // crudContextHolderService, redirectService, restService, alertService

angular.module('sw_layout')
    .factory('genericTicketService', ["$q", "alertService", "crudContextHolderService", "searchService", "userService", "applicationService", "redirectService", "restService", "$log",
        function ($q, alertService, crudContextHolderService, searchService, userService, applicationService, redirectService, restService, $log) {

    var updateTicketStatus = function (datamap) {
        // If the status is new and the user has set the owner/owner group, update the status to queued
        if (datamap['status'] == 'NEW' && (datamap['owner'] != null || datamap['ownergroup'] != null)) {
            datamap['status'] = 'QUEUED';
        }
        return true;
    };

    return {

        /**
         * 
         * @param {} datamap needed because it can come from either a grid or a detail call
         * @param {} newStatus if not set a modal should open so that the users can pick it
         * @returns {} 
         */
        changeStatus: function (datamap,schemaId, newStatus) {

            var schema = crudContextHolderService.currentSchema();
            var fields = datamap.fields ? datamap.fields : datamap;
            var dm = {};
            if (newStatus) {
                dm["newStatus"] = newStatus;
                dm["crud"] = fields;
                return applicationService.invokeOperation(schema.applicationName, schemaId, "ChangeStatus", dm).then(function(httpResponse) {
                    fields["status"] = newStatus;
                });
            }
            return $q.when();
        },

        adjustOrgId: function (event) {
            event.fields.orgid = event.fields.extrafields["site_.orgid"];
        },

        changePriority: function (datamap, schemaId, priorityField, newPriority) {
            var schema = crudContextHolderService.currentSchema();
            var fields = datamap.fields ? datamap.fields : datamap;
            var dm = fields;
            var extraParameters = {
                id: fields.ticketid
            }
            if (newPriority) {
                dm[priorityField] = newPriority;
                return applicationService.invokeOperation(schema.applicationName, schemaId, "ChangePriority", dm, extraParameters).then(function (httpResponse) {
                    fields[priorityField] = newPriority;
                });
            }
            return $q.when();
        },

        handleStatusChange: function (schema, datamap, parameters) {
            updateTicketStatus(datamap);
            if (datamap['status'] != parameters.originaldatamap['status']) {
                datamap['#hasstatuschange'] = true;
            }
        },

        beforeChangeLocation: function (event) {
            if (event.fields['assetnum'] == null) {
                //if no asset is selected we can proceed.
                return true;
            }
            if (event.fields['asset_.location'] == event.newValue) {
                //if it´s changing to the asset location, proceed. It might be due to the AfterChangeAsset callback.
                return true;
            }

            alertService.confirm(null, null, function () {
                event.fields['assetnum'] = null;
                //TODO: this should be done using watchers, so that we could remove scope from event, decoupling things
                event.scope.lookupAssociationsCode['assetnum'] = null;
                event.scope.lookupAssociationsDescription["assetnum"] = null;

                event.continue();
            }, "The location you have entered does not contain the current asset. Would you like to remove the current asset from the ticket?", function () {
                event.interrupt();
            });
        },

        beforeChangeStatus: function (event) {
            if (event.fields['owner'] == null || event.fields['status'] != "NEW") {
                return true;
            }

            alertService.confirm(null, null, function () {
                event.fields['owner'] = null;
                event.continue();
            }, "Changing the status to new would imply in removing the owner of this Service Request. Proceeed?", function () {
                event.interrupt();
            });

        },

        afterChangeAsset: function (event) {
            if (event.fields.assetnum == null) {
                return;
            }

            var assetLocation = event.fields["asset_.location"];
            var location = event.fields["location"];
            if (assetLocation !== location) {
                event.fields["location"] = assetLocation;
            }
        },

        afterchangeowner: function (event) {
            if (event.fields['owner'] == null) {
                return;
            }
            if (event.fields['owner'] == ' ') {
                event.fields['owner'] = null;
                return;
            }
            if (event.fields['status'] == 'WAPPR') {
                //event.fields['status'] = 'QUEUED';
                alertService.alert("Owner Group Field will be disabled if the Owner is selected.");
                return;
            }
            

        },

        afterchangeownergroup: function (event) {

            if (event.fields['ownergroup'] == null) {
                return;
            }
            if (event.fields['ownergroup'] == ' ') {
                event.fields['ownergroup'] = null;
                return;
            }
            if (event.fields['status'] == 'WAPPR') {
                //event.fields['status'] = 'QUEUED';
                alertService.alert("Owner Field will be disabled if the Owner Group is selected.");
                return;
                
            }
        },

        beforechangeownergroup: function (event) {
            if (event.fields['owner'] != null) {
                alertService.alert("You may select an Owner or an Owner Group; not both");
            }
        },

        beforeChangeWOServiceAddress: function (event) {
            if (event.newValue == null) {
                event.fields["woaddress_"] = null;
                event.fields["woaddress_.serviceaddressid"] = null;
                event.fields["woaddress_.formattedaddress"] = "";
            }
        },

        afterChangeWOServiceAddress: function (event) { 
            event.fields["woserviceaddress_.formattedaddress"] = event.fields["woaddress_.formattedaddress"];
            event.fields["#formattedaddr"] = event.fields["woserviceaddress_.formattedaddress"];
            event.fields["#woaddress_"] = event.fields["woaddress_"];
            event.fields["#haswoaddresschange"] = true;
        }, 

        validateCloseStatus: function (schema, datamap, originalDatamap, parameters) {
            var status = originalDatamap.originaldatamap['synstatus_.description'] == null ? originalDatamap.originaldatamap['status'] : originalDatamap.originaldatamap['synstatus_.description'];

            if (status.equalIc('CLOSED') || status.equalIc('CLOSE')) {
                alertService.alert("You cannot submit this ticket because it is already closed.");
                return false;
            }

            if (schema.applicationName == "workorder" && datamap['status'].equalIc('COMP')) {
                //TODO: extract this to a customer specific service
                if (!datamap["multiassetlocci_"]) {
                    return true;
                }
                var anyIncomplete = datamap["multiassetlocci_"].some(function(currentValue) {
                    return (currentValue.progress2 == "0" || currentValue.progress2 == 0);
                });
                if (anyIncomplete) {
                    alertService.alert("You must complete all tasks before changing WO status to Complete.");
                    return false;
                }
            }
        },

        afterChangeReportedBy: function (event) {
            var datamap = event.fields;
            var searchData = {
                personid: datamap['reportedby'],
                isprimary: '1'
            };
            searchService.searchWithData("email", searchData, "list").success(function (data) {
                var resultObject = data.resultObject[0];
                datamap['reportedemail'] = resultObject ? resultObject.fields['emailaddress'] : '';
            });
            searchService.searchWithData("phone", searchData, "list").success(function (data) {
                var resultObject = data.resultObject[0];
                datamap['reportedphone'] = resultObject ? resultObject.fields['phonenum'] : '';
            });
        },

        isDeleteAllowed: function (datamap, schema) {
            return datamap.fields['status'] === 'NEW' && datamap.fields['reportedby'] === userService.getPersonId();
        },

        isClosed: function () {
            const datamap = crudContextHolderService.originalDatamap();
            if (!datamap) {
                return false;
            }

            var originalDM = datamap.fields;
            if (!originalDM) {
                return false;
            }

            return 'CLOSED'.equalIc(originalDM['status']) || 'CLOSE'.equalIc(originalDM['status']);
        },

        //#region batch status change
        hasSelectedItemsForBatchStatus: function () {
            return Object.keys(crudContextHolderService.getSelectionModel().selectionBuffer).length > 0;
        },

        validateBatchStatusChange: function (selectedItems) {
            // check if user selected at least one entry
            if (selectedItems.length <= 0) {
                alertService.alert("Please select at least one entry to proceed.");
                return false;
            }

            // check if user selected items with different status
            const differentStatus = selectedItems.map( item => item["status"]).distinct();
            const hasDifferentStatus = differentStatus.length > 1;

            if (hasDifferentStatus) {
                const statusForMessage = differentStatus.map(s =>  `'${s}'`).join(", ");
                alertService.alert(
                    "You selected entries with status values of {0}.".format(statusForMessage) +
                    "<br>" +
                    "Please select entries with the same status to proceed."
                    );
                return false;
            }

            return true;
        },

        initBatchStatusChange: function (schema, datamap) {
            var log = $log.get("genericTicketService#initBatchStatus", ["batch"]);

            var application = schema.applicationName;
            var schemaId = schema.schemaId;

            // items selected in the buffer
            const selectedItems = Object.values(crudContextHolderService.getSelectionModel().selectionBuffer).map(s => s.fields);

            // invalid selection
            if (!this.validateBatchStatusChange(selectedItems)) return;

            log.debug("initializing batch status change for [application: {0}, schema: {1}]".format(application, schemaId));

            redirectService.openAsModal(application, "batchStatusChangeModal", {
                savefn: function (modalData, modalSchema) {
                    var newStatus = modalData["status"];

                    // only changed data + ids
                    const itemsToSubmit = selectedItems.map(selected => {
                        var dehydrated = { status: newStatus };
                        dehydrated[schema.idFieldName] = selected[schema.idFieldName];
                        dehydrated[schema.userIdFieldName] = selected[schema.userIdFieldName];
                        dehydrated["siteid"] = selected["siteid"];
                        dehydrated["orgid"] = selected["orgid"];
                        return dehydrated;
                    });

                    log.debug("submitting:", itemsToSubmit);

                    return restService.post("StatusBatch", "ChangeStatus", { application: application }, itemsToSubmit)
                        .then(() => {
                            log.debug("clearing selection buffer and realoading [application: {0}, schema: {1}]".format(application, schemaId));
                            crudContextHolderService.clearSelectionBuffer();
                            return searchService.refreshGrid(null, null, { panelid: null, keepfilterparams: true });
                        });
                }
            });
        }
        //#endregion
    };

}]);

})(angular);
