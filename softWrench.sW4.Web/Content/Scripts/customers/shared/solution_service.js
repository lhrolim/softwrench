(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('solutionService', ["redirectService", "alertService", function (redirectService, alertService) {

    return {

        //afterchange
        setsolutiondata: function (event) {
            var fields = event.fields;
            if (event.triggerparams.phase == 'initial') {
                //the first time this method is called, it will 
                return;
            }

            var solutionSympton = fields.extrafields["solution_.symptom_.ldtext"];
            var solutionCause = fields.extrafields["solution_.cause_.ldtext"];
            var solutionResolution = fields.extrafields["solution_.resolution_.ldtext"];

            fields["symptom_.ldtext"] = solutionSympton;
            fields["cause_.ldtext"] = solutionCause;
            fields["resolution_.ldtext"] = solutionResolution;
        },

        validateStatus: function (schema, datamap, originalDatamap, parameters) {
            var status = originalDatamap.originaldatamap['status'];

            if (status.equalIc('DRAFT') && datamap.status.equalIc('INACTIVE')) {
                alertService.alert("You can only update the status of this solution to active");
                return false;
            }

            if (status.equalIc('ACTIVE') && datamap.status.equalIc('DRAFT')) {
                alertService.alert("You can only update the status of this solution to inactive");
                return false;
            }

            if (status.equalIc('ACTIVE') && !datamap.status.equalIc("INACTIVE")) {
                alertService.alert("You cannot update this solution because it is active");
                return false;
            }
        },
    };

}]);

})(angular);