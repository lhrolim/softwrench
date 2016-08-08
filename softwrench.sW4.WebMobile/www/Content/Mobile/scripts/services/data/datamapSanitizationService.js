(function (mobileServices, angular) {
    "use strict";

    mobileServices.factory("datamapSanitizationService", service);

    function service() {
        
        // holds the sanitizer functions to be applied to a datamap
        var sanitizationPipeline = [
            // escapes single quotes
            function (datamap) {
                return replaceAll(datamap, "'", "\\'");
            }
            //, future sanitization processes
        ];

        var api = {
            sanitize: sanitize,
            enforceNumericType: enforceNumericType
        };

        return api;

        function enforceNumericType(datamap, displayables) {
            if (!datamap || !displayables) {
                return;
            }
            angular.forEach(displayables, function (field) {
                if (field.rendererType !== "numericinput" || field.rendererParameters["numberType"] === "double" || field.rendererParameters["numberType"] === "float") {
                    return;
                }
                if (!datamap[field.attribute]) {
                    return;
                }
                datamap[field.attribute] = parseInt(datamap[field.attribute]);
            });
        };

        function sanitize(datamap) {
            if (!datamap) {
                return datamap;
            }
            var sanitizedDataMap = angular.isString(datamap) ? angular.copy(datamap) : JSON.stringify(datamap);
            // executes each sanitizer passing the result to the next in line
            sanitizationPipeline.forEach(function (sanitizer) {
                sanitizedDataMap = sanitizer(sanitizedDataMap);
            });
            return sanitizedDataMap;
        };


    };

    // service.$inject = [];



})(mobileServices, angular);
