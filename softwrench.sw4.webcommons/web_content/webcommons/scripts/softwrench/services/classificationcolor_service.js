﻿(function(modules) {
    "use strict";

    modules.webcommons.factory('classificationColorService', ["$rootScope", "contextService", function ($rootScope, contextService) {

    //var fallbackFunction = function (status, applicationname) {
    //    if (status.equalsAny("DSME")) {
    //        return "#e59323"; //orange
    //    }

    //    if (status.equalsAny("")) {
    //        return "#fde62f"; //yellow
    //    }

    //    if (status.equalsAny("")) {
    //        return "#f65752"; //red
    //    }

    //    if (status.equalsAny("")) {
    //        return "#4488f2"; //blue
    //    }

    //    if (status.equalsAny("")) {
    //        return "#39b54a"; //green
    //    }

    //    if (status.equalsAny("")) {
    //        return "white";
    //    }

    //    return "";
    //};

    return {

        getColor: function (classification, applicationname) {
            //console.log(classification, applicationname);

            if (!classification) {
                return "";
            }

            var statuscolorJson = $rootScope.statusColor;
            //console.log($rootScope);
            //if (statuscolorJson === undefined) {
            //    //cache
            //    statuscolorJson = contextService.fetchFromContext("statuscolor", true);
            //    $rootScope.statusColor = statuscolorJson;
            //}
            //if (statuscolorJson == null) {
            //    return fallbackFunction(status, applicationname);
            //}
            //var applicationObject = statuscolorJson[applicationname];
            //if (applicationObject == null) {
            //    return fallbackFunction(status, applicationname);
            //}
            //if (status.toLowerCase() in applicationObject) {
            //    return applicationObject[status.toLowerCase()];
            //}
            //if (status in applicationObject) {
            //    return applicationObject[status];
            //}
            return "#f00";
        },

        load: function (jsonString) {
            //console.log('load', jsonString);
            contextService.insertIntoContext("classificationcolor", jsonString);
        }

        ///// <summary>
        ///// convert named color or #rgb into #rrggbb format
        ///// </summary>
        ///// <param name="color">color value or named color</param>
        ///// <returns type="tring">
        ///// #rrggbb hex color
        ///// </returns>
        //colorToHex: function (color) {
        //    //create a test element
        //    var a = document.createElement('div');
        //    a.style.color = color;

        //    //get the color parts
        //    var colors = window.getComputedStyle(document.body.appendChild(a)).color.match(/\d+/g).map(function (a) {
        //        return parseInt(a, 10);
        //    });

        //    //remove the test element
        //    document.body.removeChild(a);

        //    //make sure the hex color is in #RRGGBB format
        //    return (colors.length >= 3) ? '#' + (((1 << 24) + (colors[0] << 16) + (colors[1] << 8) + colors[2]).toString(16).substr(1)) : null;
        //},

        ///// <summary>
        ///// calculate foreground color based on background color value
        ///// based on https://24ways.org/2010/calculating-color-contrast/
        ///// </summary>
        ///// <param name="hex">color value</param>
        ///// <returns type="string">
        ///// hex color value
        ///// </returns>
        //foregroundColor: function (color) {
        //    var backgroundRGB = this.colorToHex(color);

        //    //default to black foreground color
        //    if (!backgroundRGB) {
        //        return '#000';
        //    }

        //    return parseInt(backgroundRGB.substring(1), 16) > 0xffffff / 2 ? '#000' : '#fff';
        //}
    };
}]);

})(modules);