(function (angular) {
    "use strict";

    // service.$inject = [];

    var service = function () {

        var buildMergedDatamap = function (datamap, parentdata) {
            var toClone = parentdata;
            if (parentdata.fields) {
                toClone = parentdata.fields;
            }

            var clonedDataMap = angular.copy(toClone);
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

    angular.module("sw_layout").factory("compositionCommons", service);

})(angular);
