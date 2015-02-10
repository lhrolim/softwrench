var app = angular.module('sw_layout');

app.factory('genericTicketService', function (alertService, associationService, fieldService) {

    return {

        handleStatusChange:function(schema, datamap, parameters) {
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
            if (event.fields['status'] == 'NEW') {
                event.fields['status'] = 'QUEUED';
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
            if (event.fields['status'] == 'NEW') {
                event.fields['status'] = 'QUEUED';
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

        validateCloseStatus: function (schema, datamap, parameters) {
            if (parameters.originaldatamap['status'] == 'CLOSE') {
                alertService.alert("You cannot submit this ticket because it is already closed");
                return false;
            }
        }
    };

});