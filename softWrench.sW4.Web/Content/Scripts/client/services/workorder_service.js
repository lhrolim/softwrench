var app = angular.module('sw_layout');

app.factory('workorderservice', function (i18NService, fieldService, $rootScope, dispatcherService) {

    return {

        defaultvalidation: function(schema, displayables) {
            return; 
        },

        comvalidation: function (schema, displayables) {
            // COMSW-10: Must have complete asset or location fields in order to submit work order
            if (displayables["asset_"] == undefined && displayables["location"] == undefined) {
                return ["A work order must have an asset or location."];
            }

            return;
        }
    };
});
