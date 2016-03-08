(function(modules) {
    "use strict";

    modules.webcommons.factory("classificationColorService", ["$rootScope", "contextService", function ($rootScope, contextService) {

        var fallbackFunction = function (classification) {

            //TODO: add classification fallback colors
            //if (status.equalsAny("")) {
            //    return "#e59323"; //orange
            //}
            //if (status.equalsAny("")) {
            //    return "#fde62f"; //yellow
            //}

            //if (status.equalsAny("")) {
            //    return "#f65752"; //red
            //}

            //if (status.equalsAny("")) {
            //    return "#4488f2"; //blue
            //}

            //if (status.equalsAny("")) {
            //    return "#39b54a"; //green
            //}

            return "";
        };

        return {
            getColor: function (classification, applicationname) {
                //if there is not classification, don't return a color
                if (!classification) {
                    return "";
                }

                var fallback = fallbackFunction(classification);

                //check for classificationcolor.json
                var classificationJson = contextService.fetchFromContext("classificationcolor", true, false, false);
                if (classificationJson == null) {
                    return fallback;
                }

                //check for application colors
                var applicationObject = classificationJson[applicationname];
                if (applicationObject == null) {

                    var applicationObject = classificationJson["default"];
                        if (applicationObject == null) {
                            return fallback;
                        }
                }

                //if the classification is found in the application
                if (classification in applicationObject) {
                    return applicationObject[classification];
                }

                return fallback;
            },

            load: function (jsonString) {
                contextService.insertIntoContext("classificationcolor", jsonString);
            }
        };
    }
]);

})(modules);