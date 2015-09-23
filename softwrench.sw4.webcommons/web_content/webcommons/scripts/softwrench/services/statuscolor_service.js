(function(modules) {
    "use strict";

modules.webcommons.factory('statuscolorService', ["$rootScope", "contextService", function ($rootScope, contextService) {

    var fallbackFunction = function (status, applicationname) {

        if (status.equalsAny("NEW", "WAPPR", "WSCH", "ACTIVE")) {
            return "#e59323"; //orange
        }
        if (status.equalsAny("QUEUED", "INPROG", "CANTREPROD", "WAITONINFO", "PENDING", "WMATL", "WORKING", "null")) {
            return "#fde62f"; //yellow
        }

        if (status.equalsAny("CANCELLED", "FAIL", "CAN", "FAILPIR", "REJECTED", "NOTREQ", "WONT FIX", "WONTIMPLNT", "WONTRESPND", "POSTPONED", "SPAM" )) {
            return "#f65752"; //red
        }

        if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "APPFM", "APPLM", "BY DESIGN", "AUTHORIZED", "DUPLICATE", "AUTH", "FIXED", "HOLDINPRG", "INPRG", "PLANNED", "ACC_CAT", "ASSESSES")) {
            return "#4488f2"; //blue
        }
        if (status.equalsAny("CLOSED", "IMPLEMENTED", "RESOLVCONF", "IMPL", "REVIEW", "CLOSE", "HISTEDIT", "COMP", "COMPLETED", "INPRG", "PLANNED")) {
            return "#39b54a"; //green
        }
        if (status.equalsAny("DRAFT")) {
            return "white";
        }
        return "transparent";
    };

    return {

        getColor: function (status, applicationname) {
            if (!status) {
                return "transparent";
            }

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

}]);

})(modules);