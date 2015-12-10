(function (angular) {
    "use strict";

    // service.$inject = [];

    var service = function () {

        var sanitizeDataMapToSendOnAssociationFetching = function (datamap) {
            var result = {};
            angular.forEach(datamap, function (value, key) {
                if (key.indexOf("_") !== -1) {
                    //ignoring any relationship data
                    return;
                }
                result[key] = value;
            });
            return result;
        }

        var api = {
            sanitizeDataMapToSendOnAssociationFetching: sanitizeDataMapToSendOnAssociationFetching
        };

        return api;
    };

    angular.module("webcommons_services").factory("datamapSanitizeService", service);

})(angular);
