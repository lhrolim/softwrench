﻿var app = angular.module('sw_layout');

app.factory('glcomponentService', function ($http, modalService, validationService) {
    return {
        // A new getGLAccount() is needed for the client profile to get their customization.
        // Have the metadata.xml redirect to that service and function.
        getGLAccount: function (datamap, schema, selecteditem, attribute) {

            // refer to glconfigure table to program this section
            // this will be different for each account
            var result = selecteditem["glpart-0"] == undefined ? "????" : selecteditem["glpart-0"];
            result += selecteditem["glpart-1"] == undefined ? "-???" : "-" + selecteditem["glpart-1"];
            result += selecteditem["glpart-2"] == undefined ? "-???" : '-' + selecteditem["glpart-2"];
            result += selecteditem["glpart-3"] == undefined ? "" : '-' + selecteditem["glpart-3"];
            // Output results to the parent schema

            datamap[attribute.target] = result;

            // close the modal
            modalService.hide();
        }
    };
});