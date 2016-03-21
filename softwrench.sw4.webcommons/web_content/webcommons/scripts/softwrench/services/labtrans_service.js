(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('labtranService', ["$q", "alertService", function ($q,alertService) {
    
    return {
        afterlaborchange: function (event) {
            if (event.fields['laborcode'] == ' ') {
                event.fields['craft'] = null;
                event.fields['payrate'] = 0.0; // Reset payrate
                alertService.alert("Task field will be disabled if labor is not selected");
            }
            // TODO: Need to reset the display fields on craft after laborcode has been changed.
            return;
        },
        aftercraftchange: function (event) {
            if (event.fields['laborcraftrate_.rate'] != null) {
                event.fields['payrate'] = event.fields['laborcraftrate_.rate'];
                event.fields['linecost'] = event.fields['laborcraftrate_.rate'] * event.fields['regularhrs'];
            }
            else {
                event.fields['payrate'] = 0.0;
                event.fields['linecost'] = 0.0;
            }
        },
        afterDateTimeChange: function (event) {
            // If all of the datetime fields are filed
            if ((event.fields['startdate'] && !event.fields['startdate'].nullOrEmpty()) &&
                (event.fields['starttime'] && !event.fields['starttime'].nullOrEmpty()) &&
                (event.fields['finishdate'] && !event.fields['finishdate'].nullOrEmpty()) &&
                (event.fields['finishtime'] && !event.fields['finishtime'].nullOrEmpty())) {

                var startDate = new Date(event.fields['startdate']);
                var startTime = Date.parse(event.fields['starttime']);
                startDate.setHours(startTime.getHours());
                startDate.setMinutes(startTime.getMinutes());

                var finishDate = new Date(event.fields['finishdate']);
                var finishTime = Date.parse(event.fields['finishtime']);
                finishDate.setHours(finishTime.getHours());
                finishDate.setMinutes(finishTime.getMinutes());

                // time diff
                var difference = finishDate - startDate;
                // convert ms to hours
                var hours = difference / 3600000;
                hours = hours.toPrecision(6);
                // set the labor hours
                event.fields['regularhrs'] = hours;
            }
        },

        validateRemoval: function (datamap, schema) {
            if (datamap["genapprservreceipt"] === 1) {
                alertService.alert("Cannot remove already approved labor transactions");
                return $q.reject();
            }
            return $q.when();

        }

    };
}]);

})(angular);