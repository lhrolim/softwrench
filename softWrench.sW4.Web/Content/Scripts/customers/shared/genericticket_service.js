﻿var app = angular.module('sw_layout');

app.factory('genericTicketService', function (alertService, associationService, fieldService) {

    var updateTicketStatus = function (datamap) {
        // If the status is new and the user has set the owner/owner group, update the status to queued
        if (datamap['status'] == 'NEW' && (datamap['owner'] != null || datamap['ownergroup'] != null)) {
            datamap['status'] = 'QUEUED';
        }
        return true;
    };

    return {

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
            if (event.oldValue == null && event.fields['asset_.location'] == event.newValue) {
                //if the location was null, and now it´s changing to the asset location, proceed. It might be 
                //due to the AfterChangeAsset callback.
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
            if (event.fields['location'] == null && event.fields.assetnum != null) {
                var location = event.fields['asset_.location'];
                event.fields['location'] = location;
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
        }, 

        validateCloseStatus: function (schema, datamap, originalDatamap,parameters) {
            if (originalDatamap.originaldatamap['synstatus_.description'].equalIc('CLOSED') || originalDatamap.originaldatamap['synstatus_.description'].equalIc('CLOSE')) {
                alertService.alert("You cannot submit this ticket because it is already closed");
                return false;
            }
        },

        checkWorklogsForChange: function(schema, datamap, parameters) {
            var worklogs = datamap.worklog_;
            var worklogsOriginal = parameters.originaldatamap.worklog_;
            for(var worklog in worklogs) {
                var hasChanged = 0;
                if (worklog < worklogsOriginal.length) {
                    if (worklogs[worklog].description != worklogsOriginal[worklog].description
                        || worklogs[worklog]["longdescription_.ldtext"] != worklogsOriginal[worklog]["longdescription_.ldtext"]) {
                        hasChanged = 1;
                    }
                } else {
                    hasChanged = 1;
                }
                datamap.worklog_[worklog]['#hasChanged'] = hasChanged;
            }
        }
    };

});