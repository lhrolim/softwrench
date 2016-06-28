(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('previousFilterService', ["contextService", function (contextService) {

            function createPreviousFilter(schema, searchData, searchOperators, searchSort) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                var fields = "";
                var operators = "";
                var values = "";
                var fieldsValueDelimiter = ",";
                var valueValueDelimiter = ",,,";
                for (var data in searchData) {
                    if (data == "lastSearchedValues") {
                        continue;
                    }
                    fields += data + fieldsValueDelimiter;
                    values += searchData[data] + valueValueDelimiter;
                    var operator = searchOperators[data];
                    // the operators are only setted if the user specific select them, but they should be 'C' as default --> avoiding inconsistency
                    operators += (operator == null ? 'C' : operator.symbol) + ",";
                }
                // Check if search sort is not blank
                if (fields === "" && values === "") {
                    return;
                }
                // The previous filter needs to have an ID that will never be used by the regular filter creation methods
                var previousFilterId = -2;
                // The previous filter must be given an alias to make it stand out from the user created filters
                var previousFilterAlias = "*Previous Unsaved Filter*";
                var filter = {
                    applicationKey: applicationKey,
                    fields: fields.substr(0, fields.length - 1),
                    operators: operators.substr(0, operators.length - 1),
                    values: values.substr(0, values.length - 3),
                    searchSort: searchSort,
                    template: "",
                    advancedSearch: "",
                    alias: previousFilterAlias,
                    id: previousFilterId
                };
                var previousFilters = contextService.fetchFromContext('previousFilters', true, false, false);
                if (!previousFilters) {
                    previousFilters = {};
                }
                previousFilters[applicationKey] = filter;
                contextService.insertIntoContext('previousFilters', previousFilters, false);
            };

            function fetchPreviousFilter(applicationKey) {
                const previousFilterArray = this.fetchAllPreviousFilters();
                return previousFilterArray.firstOrDefault(function (item) {
                    return item.applicationKey === applicationKey;
                });
            }

            function fetchAllPreviousFilters() {
                const previousFilters = contextService.fetchFromContext('previousFilters', true, false, false);
                if (previousFilters == null) {
                    return [];
                }
                var result = Object.values(previousFilters);
                return result;
            };

            const api = {
                createPreviousFilter,
                fetchAllPreviousFilters,
                fetchPreviousFilter
            };
            return api;

        }]);

})(angular);