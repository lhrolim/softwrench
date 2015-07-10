﻿(function (angular) {
    "use strict";

    // service.$inject = [];

    var service = function () {

        var buildMergedDatamap = function (datamap, parentdata) {
            var clonedDataMap = angular.copy(parentdata);
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

    angular.module("webcommons_services").factory("compositionCommons", service);

})(angular);
