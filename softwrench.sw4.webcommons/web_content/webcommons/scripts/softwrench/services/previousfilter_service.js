(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('previousFilterService', ["contextService", "$log", function (contextService, $log) {

            function createPreviousFilter(schema, searchData, searchOperators, searchSort) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                var fields = "";
                var operators = "";
                var values = "";
                const fieldsValueDelimiter = ",";
                const valueValueDelimiter = ",,,";

                for (let data in searchData) {
                    if (data === "lastSearchedValues") {
                        continue;
                    }
                    fields += data + fieldsValueDelimiter;
                    values += searchData[data] + valueValueDelimiter;
                    const operator = searchOperators[data];
                    // the operators are only setted if the user specific select them, but they should be 'C' as default --> avoiding inconsistency
                    operators += (operator == null ? 'C' : operator.symbol) + ",";
                }
                // Check if search sort is not blank
                if (fields === "" && values === "") {
                    return;
                }
                // The previous filter needs to have an ID that will never be used by the regular filter creation methods
                const previousFilterId = -2;
                // The previous filter must be given an alias to make it stand out from the user created filters
                const previousFilterAlias = "*Previous Unsaved Filter*";
                const filter = {
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
                $log.get("previousFilterService", ["filter"]).info(`adding previousfilter for ${applicationKey}`);
                previousFilters[applicationKey] = filter;
                contextService.insertIntoContext('previousFilters', previousFilters, false);
            };

            function fetchPreviousFilter(applicationKey) {
                const previousFilterArray = this.fetchAllPreviousFilters();
                return previousFilterArray[applicationKey];
            }

            function fetchAllPreviousFilters() {
                const previousFilters = contextService.fetchFromContext('previousFilters', true, false, false);
                if (previousFilters == null) {
                    return {};
                }
                return previousFilters;
            };

            const api = {
                createPreviousFilter,
                fetchAllPreviousFilters,
                fetchPreviousFilter
            };
            return api;

        }]);

})(angular);