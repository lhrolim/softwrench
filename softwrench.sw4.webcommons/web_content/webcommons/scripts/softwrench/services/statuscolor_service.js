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

        /// <summary>
        /// convert hex color string #rrggbb or #rgb into RGB parts
        /// </summary>
        /// <param name="hex">color value</param>
        /// <returns type="object">
        /// r: int
        /// g: int
        /// b: int
        /// </returns>
        hexToRgb: function (hex) {
            var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
            hex = hex.replace(shorthandRegex, function (m, r, g, b) {
                return r + r + g + g + b + b;
            });

            var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : null;
        },

        /// <summary>
        /// calculate foreground color based on background color value
        /// based on https://24ways.org/2010/calculating-color-contrast/
        /// </summary>
        /// <param name="hex">color value</param>
        /// <returns type="string">
        /// hex color value
        /// </returns>
        foregroundColor: function (hex) {
            var backgroundRGB = this.hexToRgb(hex);

            if (!backgroundRGB) {
                //default to black foreground color
                return '#000';
            }

            return parseInt(hex.substring(1), 16) > 0xffffff / 2 ? '#000' : '#fff';
        }
    };
}]);

})(modules);