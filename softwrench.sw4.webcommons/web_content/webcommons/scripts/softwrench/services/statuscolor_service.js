(function(modules) {
    "use strict";

modules.webcommons.factory('statuscolorService', ["$rootScope", "contextService", function ($rootScope, contextService) {

    var fallbackFunction = function (status, applicationname) {

        if (status.equalsAny("NEW", "WAPPR", "WSCH", "ACTIVE")) {
            return "#e59323"; //orange
        }

        if (status.equalsAny("QUEUED", "CANTREPROD", "WAITONINFO", "PENDING", "WMATL", "WORKING", "null")) {
            return "#f2d935"; //yellow
        }

        if (status.equalsAny("CANCELLED", "FAIL", "CAN", "FAILPIR", "REJECTED", "NOTREQ", "WONT FIX", "WONTIMPLNT", "WONTRESPND", "POSTPONED", "SPAM" )) {
            return "#f65752"; //red
        }

        if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "APPFM", "APPLM", "BY DESIGN", "AUTHORIZED", "DUPLICATE", "AUTH", "FIXED", "HOLDINPRG",  "INPROG", "INPRG", "PLANNED", "ACC_CAT", "ASSESSES")) {
            return "#4488f2"; //blue
        }

        if (status.equalsAny("CLOSED", "IMPLEMENTED", "RESOLVCONF", "IMPL", "REVIEW", "CLOSE", "HISTEDIT", "COMP", "COMPLETED", "INPRG", "PLANNED")) {
            return "#39b54a"; //green
        }

        if (status.equalsAny("DRAFT")) {
            return "white";
        }
        return "#777";
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

                //since the application was not found, check for a customer default colors
                var applicationObject = statuscolorJson["default"];
                if (applicationObject == null) {
                    return fallbackFunction(status, applicationname);
                }
            }

            if (status.toLowerCase() in applicationObject) {
                return applicationObject[status.toLowerCase()];
            }

            if (status in applicationObject) {
                return applicationObject[status];
            }

            return "#777";
        },

        load: function (jsonString) {
            contextService.insertIntoContext("statuscolor", jsonString);
        },

        /// <summary>
        /// convert named color or #rgb into #rrggbb format
        /// </summary>
        /// <param name="color">color value or named color</param>
        /// <returns type="tring">
        /// #rrggbb hex color
        /// </returns>
        colorToHex: function (color) {
            //create a test element
            var a = document.createElement('div');
            a.style.color = color;

            //get the color parts
            var colors = window.getComputedStyle(document.body.appendChild(a)).color.match(/\d+/g).map(function (a) {
                return parseInt(a, 10);
            });

            //remove the test element
            document.body.removeChild(a);

            //make sure the hex color is in #RRGGBB format
            return (colors.length >= 3) ? '#' + (((1 << 24) + (colors[0] << 16) + (colors[1] << 8) + colors[2]).toString(16).substr(1)) : null;
        },

        /// <summary>
        /// calculate foreground color based on background color value
        /// based on https://24ways.org/2010/calculating-color-contrast/
        /// </summary>
        /// <param name="hex">color value</param>
        /// <returns type="string">
        /// hex color value
        /// </returns>
        foregroundColor: function (color) {
            var backgroundRGB = this.colorToHex(color);

            //default to black foreground color
            if (!backgroundRGB) {
                return '#000';
            }

            return parseInt(backgroundRGB.substring(1), 16) > 0xffffff / 2 ? '#000' : '#fff';
        },

        getPriorityColor: function (priority) {
            if (priority === 1 || priority === "1") {
                return "#39b54a"; //green
            }
            if (priority === 2 || priority === "2") {
                return "#4488f2"; //blue
            }
            if (priority === 3 || priority === "3") {
                return "#D850FA"; //purple
            }
            if (priority === 4 || priority === "4") {
                return "#f2d935"; //yellow
            }
            if (priority === 5 || priority === "5") {
                return "#e59323"; //orange
            }
            if (priority === 6 || priority === "6") {
                return "#f65752"; //red
            }
            return "#777";
        }
    };
}]);

})(modules);