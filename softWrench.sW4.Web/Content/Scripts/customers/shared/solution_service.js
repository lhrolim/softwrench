var app = angular.module('sw_layout');

app.factory('solutionService', function (redirectService) {

    return {

        setsolutiondata: function (event) {
            var fields = event.fields;
            if (event.triggerparams.phase == 'initial') {
                //the first time this method is called, it will 
                return;
            }

            var solutionSympton = fields.extrafields["solution_.symptom_.ldtext"];
            var solutionCause = fields.extrafields["solution_.cause_.ldtext"];
            var solutionResolution = fields.extrafields["solution_.resolution_.ldtext"];

            fields["sympton_.ldtext"] = solutionSympton;
            fields["cause_.ldtext"] = solutionCause;
            fields["resolution_.ldtext"] = solutionResolution;
        },

        



    };

});