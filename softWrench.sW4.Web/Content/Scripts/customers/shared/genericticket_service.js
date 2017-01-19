(function (angular) {
    "use strict";

    // crudContextHolderService, redirectService, restService, alertService

    angular.module('sw_layout')
        .service('genericTicketService', ["$q", "alertService", "crudContextHolderService", "searchService", "userService", "applicationService", "redirectService", "restService", "$log", "lookupService",
            function ($q, alertService, crudContextHolderService, searchService, userService, applicationService, redirectService, restService, $log, lookupService) {

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
                    changeStatus: function (datamap, schemaId, newStatus) {
                        const schema = crudContextHolderService.currentSchema();
                        const dm = {};
                        if (newStatus) {
                            dm["newStatus"] = newStatus;
                            dm["crud"] = datamap;
                            return applicationService.invokeOperation(schema.applicationName, schemaId, "ChangeStatus", dm).then(function (httpResponse) {
                                datamap["status"] = newStatus;
                            });
                        }
                        return $q.when();
                    },

                    //afterchange
                    adjustOrgId: function (event) {
                        event.orgid = event.fields.extrafields["site_.orgid"];
                    },

                    changePriority: function (datamap, schemaId, priorityField, newPriority) {
                        const schema = crudContextHolderService.currentSchema();
                        const dm = datamap;
                        const extraParameters = {
                            id: datamap.ticketid
                        };
                        if (newPriority) {
                            dm[priorityField] = newPriority;
                            return applicationService.invokeOperation(schema.applicationName, schemaId, "ChangePriority", dm, extraParameters).then(function (httpResponse) {
                                datamap[priorityField] = newPriority;
                            });
                        }
                        return $q.when();
                    },

                    handleStatusChange: function (schema, datamap, parameters) {
                        updateTicketStatus(datamap);
                        if (datamap['status'] !== parameters.originaldatamap['status']) {
                            datamap['#hasstatuschange'] = true;
                        }
                    },

                    //beforechange
                    beforeChangeLocation: function (event) {
                        const fields = event.fields;
                        if (fields['assetnum'] == null) {
                            lookupService.clearAutoCompleteCache("asset_");
                            //if no asset is selected we can proceed.
                            return true;
                        }
                        if (fields['asset_.location'] === event.newValue) {
                            lookupService.clearAutoCompleteCache("asset_");
                            //if it´s changing to the asset location, proceed. It might be due to the AfterChangeAsset callback.
                            return true;
                        }

                        alertService.confirm("The location you have entered does not contain the current asset. Would you like to remove the current asset from the ticket?").then(() => {
                            fields['assetnum'] = null;
                            lookupService.clearAutoCompleteCache("asset_");
                            event.continue();
                        }, function () {
                            event.interrupt();
                        });
                    },

                    //beforechange
                    beforeChangeStatus: function (event) {
                        if (event.fields['owner'] == null || event.fields['status'] !== "NEW") {
                            return true;
                        }

                        alertService.confirm("Changing the status to new would imply in removing the owner of this Service Request. Proceeed?").then(function () {
                            event.fields['owner'] = null;
                            event.continue();
                        }, function () {
                            event.interrupt();
                        });
                    },


                    //afterchange
                    afterChangeAsset: function (event) {
                        const fields = event.fields;
                        if (fields.assetnum == null) {
                            return;
                        }
                        const assetLocation = fields["asset_.location"];
                        const location = fields["location"];
                        if (assetLocation !== location) {
                            fields["location"] = assetLocation;
                        }
                    },


                    //afterchange
                    afterchangeowner: function (event) {
                        const fields = event.fields;
                        if (fields['owner'] == null) {
                            return;
                        }
                        if (fields['owner'] === ' ') {
                            fields['owner'] = null;
                            return;
                        }
                        if (event.fields['status'] === 'NEW' || event.fields['#originalstatus'] === "NEW") {
                            alertService.alert("Owner Group Field will be disabled if the Owner is selected.");
                            return;
                        }

                        if (fields['status'] === 'WAPPR') {
                            //event.fields['status'] = 'QUEUED';
                            alertService.alert("Owner Group Field will be disabled if the Owner is selected.");
                            return;
                        }


                    },

                    //afterchange
                    afterchangeownergroup: function (event) {
                        const fields = event.fields;
                        if (fields['ownergroup'] == null) {
                            return;
                        }
                        if (fields['ownergroup'] === ' ') {
                            fields['ownergroup'] = null;
                            return;
                        }

                        if (fields["status"] === "NEW" || fields['status'] === 'WAPPR') {
                            //event.fields['status'] = 'QUEUED';
                            alertService.alert("Owner Field will be disabled if the Owner Group is selected.");
                            return;

                        }
                    },

                    //beforechange
                    beforechangeownergroup: function (event) {
                        const fields = event.fields;
                        if (fields['owner'] != null) {
                            alertService.alert("You may select an Owner or an Owner Group; not both");
                        }
                    },
                    //beforechange
                    beforeChangeWOServiceAddress: function (event) {
                        const fields = event.fields;
                        if (event.newValue == null) {
                            fields["woaddress_"] = null;
                            fields["woaddress_.serviceaddressid"] = null;
                            fields["woaddress_.formattedaddress"] = "";
                        }
                    },

                    //afterchange
                    afterChangeWOServiceAddress: function (event) {
                        const fields = event.fields;
                        fields["woserviceaddress_.formattedaddress"] = fields["woaddress_.formattedaddress"];
                        fields["#formattedaddr"] = fields["woserviceaddress_.formattedaddress"];
                        fields["#woaddress_"] = fields["woaddress_"];
                        fields["#haswoaddresschange"] = true;
                    },

                    validateCloseStatus: function (schema, datamap, parameters) {
                        const status = parameters.originaldatamap['synstatus_.description'] == null ? parameters.originaldatamap['status'] : parameters.originaldatamap['synstatus_.description'];
                        if (status.equalIc('CLOSED') || status.equalIc('CLOSE')) {
                            alertService.alert("You cannot submit this ticket because it is already closed.");
                            return false;
                        }

                        if (schema.applicationName == "workorder" && datamap['status'].equalIc('COMP')) {
                            //TODO: extract this to a customer specific service
                            if (!datamap["multiassetlocci_"]) {
                                return true;
                            }
                            const anyIncomplete = datamap["multiassetlocci_"].some(function (currentValue) {
                                return (currentValue.progress2 == "0" || currentValue.progress2 == 0);
                            });
                            if (anyIncomplete) {
                                alertService.alert("You must complete all tasks before changing WO status to Complete.");
                                return false;
                            }
                        }
                    },

                    //afterchange
                    afterChangeReportedBy: function (event) {
                        var datamap = event.fields;
                        const searchData = {
                            personid: datamap['reportedby'],
                            isprimary: '1'
                        };
                        const p1 = searchService.searchWithData("email", searchData, "list").then(function (response) {
                            const data = response.data;
                            const resultObject = data.resultObject[0];
                            datamap['reportedemail'] = resultObject ? resultObject['emailaddress'] : '';
                        });
                        const p2 = searchService.searchWithData("phone", searchData, "list").then(function (response) {
                            const data = response.data;
                            const resultObject = data.resultObject[0];
                            datamap['reportedphone'] = resultObject ? resultObject['phonenum'] : '';
                            return $q.when();
                        });
                        return $q.all([p1, p2]).catch(err=>console.log(err));
                    },

                    isDeleteAllowed: function (datamap, schema) {
                        return datamap['status'] === 'NEW' && datamap['reportedby'] === userService.getPersonId().toUpperCase();
                    },

                    isClosed: function () {
                        const datamap = crudContextHolderService.originalDatamap();
                        if (!datamap) {
                            return false;
                        }
                        let status = datamap["status"];
                        if (datamap.hasOwnProperty("originalstatus")) {
                            status = datamap["originalstatus"];
                        }
                        return status && status.equalsAny("CLOSE", "CLOSED");
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
                        const differentStatus = selectedItems.map(item => item["status"]).distinct();
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
                        const selectedItems = Object.values(crudContextHolderService.getSelectionModel().selectionBuffer).map(s => s);

                        // invalid selection
                        if (!this.validateBatchStatusChange(selectedItems)) return;

                        log.debug("initializing batch status change for [application: {0}, schema: {1}]".format(application, schemaId));

                        const status = selectedItems.map(s => s["status"])[0];

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
                            },
                            onloadfn: function (modalScope) {
                                modalScope.datamap["originalstatus"] = status;
                                modalScope.datamap["addcurrent"] = false;
                            }

                        });
                    }
                    //#endregion
                };

            }]);

})(angular);
