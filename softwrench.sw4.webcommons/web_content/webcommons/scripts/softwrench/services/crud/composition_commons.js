(function (angular) {
    "use strict";

    // service.$inject = [];

    var service = function (datamapSanitizeService) {

        /**
         * merges the parent datamap with a specific composition row data, making sure that, 
         * in case of conflicts, the composition data is sent and not the parent one (ex: both have assets)
         * @param {} datamap 
         * @param {} parentdata 
         * @returns {} 
         */
        var buildMergedDatamap = function (datamap, parentdata) {
            var toClone = parentdata;
            
            var clonedDataMap = angular.copy(toClone);
            clonedDataMap = datamapSanitizeService.sanitizeDataMapToSendOnAssociationFetching(clonedDataMap);

            if (datamap) {
                var item = datamap;
                for (var prop in item) {
                    if (item.hasOwnProperty(prop)) {
                        clonedDataMap[prop] = item[prop];
                    }
                }
            }
            return clonedDataMap;
        };

        var api = {
            buildMergedDatamap: buildMergedDatamap
        };

        return api;
    };

    angular.module("webcommons_services").service("compositionCommons", ['datamapSanitizeService', service]);

})(angular);
