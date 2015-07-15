(function (mobileServices, angular) {
    "use strict";

    var service = function () {

        // holds the sanitizer functions to be applied to a datamap
        var sanitizationPipeline = [
            // escapes single quotes
            function (datamap) {
                return replaceAll(datamap, "'", "\\'");
            }
            //, future sanitization processes
        ];

        var sanitize = function(datamap) {
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

        var api = {
            sanitize: sanitize
        };

        return api;
    };

    // service.$inject = [];

    mobileServices.factory("datamapSanitizationService", service);

})(mobileServices, angular);
