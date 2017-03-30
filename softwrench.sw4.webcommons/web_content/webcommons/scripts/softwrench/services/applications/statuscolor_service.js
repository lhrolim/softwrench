﻿(function(modules) {
    "use strict";

modules.webcommons.service('statuscolorService', ["$rootScope", "contextService", function ($rootScope, contextService) {

    var fallbackFunction = function (status, applicationname) {

        //if (status.equalsAny("NEW", "WAPPR", "WSCH", "ACTIVE")) {
        //    return "#e59323"; //orange
        //}

        //if (status.equalsAny("QUEUED", "CANTREPROD", "WAITONINFO", "PENDING", "WMATL", "WORKING", "null")) {
        //    return "#f2d935"; //yellow
        //}

        //if (status.equalsAny("CANCELLED", "FAIL", "CAN", "FAILPIR", "REJECTED", "NOTREQ", "WONT FIX", "WONTIMPLNT", "WONTRESPND", "POSTPONED", "SPAM" )) {
        //    return "#f65752"; //red
        //}

        //if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "APPFM", "APPLM", "BY DESIGN", "AUTHORIZED", "DUPLICATE", "AUTH", "FIXED", "HOLDINPRG",  "INPROG", "INPRG", "PLANNED", "ACC_CAT", "ASSESSES")) {
        //    return "#4488f2"; //blue
        //}

        //if (status.equalsAny("CLOSED", "IMPLEMENTED", "RESOLVCONF", "IMPL", "REVIEW", "CLOSE", "HISTEDIT", "COMP", "COMPLETED", "INPRG", "PLANNED")) {
        //    return "#39b54a"; //green
        //}

        //if (status.equalsAny("DRAFT")) {
        //    return "white";
        //}
        //return "#777";

        var statuscolorFallbackJson = $rootScope.statuscolorFallbackJson;
        if (!statuscolorFallbackJson) {
            statuscolorFallbackJson = contextService.fetchFromContext("statuscolorfallback", true);
            $rootScope.statuscolorFallbackJson = statuscolorFallbackJson;
        }

        if (!statuscolorFallbackJson || !applicationname) {
            return "#777";
        }

        var applicationObject = statuscolorFallbackJson[applicationname];
        if (!applicationObject) {
            //if there is no default application in the json, stop here
            applicationObject = statuscolorFallbackJson["default"];
        }

        return applicationObject[status.toLowerCase()];
    };

    return {

        getColor: function (status, applicationname) {
            if (!status) {
                return "transparent";
            }

            var statuscolorJson = $rootScope.statusColor;
            if (statuscolorJson === undefined) {
                statuscolorJson = contextService.fetchFromContext("statuscolor", true);
                $rootScope.statusColor = statuscolorJson;
            }

            //if the color json is missing, stop here
            if (statuscolorJson == null) {
                return fallbackFunction(status, applicationname);
            }

            //if the application is missing from the json file
            var applicationObject = statuscolorJson[applicationname];
            if (!applicationObject) {
                //if there is no default application in the json, stop here
                applicationObject = statuscolorJson["default"];
                if (!applicationObject) {
                    return fallbackFunction(status, applicationname);
                }
            }

            //check for the status in the application/default
            if (status in applicationObject || status.toLowerCase() in applicationObject) {
                return applicationObject[status.toLowerCase()];
            }

            //nothing else worked
            return fallbackFunction(status, applicationname);
        },

        load: function (jsonString, fallbackJson) {
            contextService.insertIntoContext("statuscolor", jsonString);
            contextService.insertIntoContext("statuscolorfallback", fallbackJson);
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