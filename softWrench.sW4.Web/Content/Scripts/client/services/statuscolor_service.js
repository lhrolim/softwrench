var app = angular.module('sw_layout');

app.factory('statuscolorService', function ($rootScope, contextService) {

    var fallbackFunction = function (status, applicationname) {

        if (status.equalsAny("NEW", "WAPPR", "WSCH")) {
            return "orange";
        }
        if (status.equalsAny("QUEUED", "INPROG", "PENDING","null")) {
            return "yellow";
        }

        if (status.equalsAny("CANCELLED", "FAIL", "CAN", "FAILPIR", "REJECTED", "NOTREQ")) {
            return "red";
        }

        if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "AUTHORIZED", "AUTH", "HOLDINPRG", "INPRG", "PLANNED", "ACC_CAT", "ASSESSES")) {
            return "blue";
        }
        if (status.equalsAny("CLOSED", "RESOLVCONF", "IMPL", "REVIEW", "CLOSE", "HISTEDIT", "COMP", "INPRG", "PLANNED")) {
            return "green";
        }
        if (status.equalsAny("DRAFT")) {
            return "white";
        }
        return "transparent";
    };

    return {

        getColor: function (status, applicationname) {
            var statuscolorJson = $rootScope.statusColor;
            if (statuscolorJson === undefined) {
                //cache
                statuscolorJson = contextService.fetchFromContext("statuscolor", true);
                $rootScope.statusColor = statuscolorJson;
            }
            if (statuscolorJson == null) {
                return fallbackFunction(status, applicationname);
            }
            var applicationObject = statuscolorJson[applicationname];
            if (applicationObject == null) {
                return fallbackFunction(status, applicationname);
            }
            if (status.toLowerCase() in applicationObject) {
                return applicationObject[status.toLowerCase()];
            }
            if (status in applicationObject) {
                return applicationObject[status];
            }
            return "transparent";
        },

        load: function (jsonString) {
            contextService.insertIntoContext("statuscolor", jsonString);
        },





    };

});


