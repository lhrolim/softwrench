(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('previousFilterService', ["contextService", "$log", function (contextService, $log) {

            const fieldsValueDelimiter = ",";
            const valueValueDelimiter = ",,,";

            function createPreviousFilter(schema, searchData, searchOperators, searchSort) {
                const applicationKey = schema.applicationName + "." + schema.schemaId;
                var fields = "";
                var operators = "";
                var values = "";
                let anyFilter = false;
            

                for (let data in searchData) {
                    if (searchData.hasOwnProperty(data)) {
                        if (data === "lastSearchedValues") {
                            continue;
                        }
                        anyFilter = true;
                        fields += data + fieldsValueDelimiter;
                        values += searchData[data] + valueValueDelimiter;
                        const operator = searchOperators[data];
                        // the operators are only setted if the user specific select them, but they should be 'C' as default --> avoiding inconsistency
                        operators += (operator == null ? 'C' : operator.symbol) + ",";
                    }
                }

                // Check if search sort is not blank
                if (!anyFilter && !searchSort) {
                    return;
                }
                // The previous filter needs to have an ID that will never be used by the regular filter creation methods
                const previousFilterId = -2;
                // The previous filter must be given an alias to make it stand out from the user created filters
                const previousFilterAlias = "*Previous Unsaved Filter*";

                const filter = {
                    applicationKey: applicationKey,
                    fields: anyFilter ? fields.substr(0, fields.length - 1) : null,
                    operators: anyFilter ? operators.substr(0, operators.length - 1) : null,
                    values: anyFilter ? values.substr(0, values.length - 3) : null,
                    searchSort: searchSort,
                    template: "",
                    advancedSearch: "",
                    alias: previousFilterAlias,
                    id: previousFilterId
                };
                const previousFilters = contextService.fetchFromContext('previousFilters', true, false, false) || {};
                
                $log.get("previousFilterService", ["filter"]).info(`adding previousfilter for ${applicationKey}`);
                previousFilters[applicationKey] = filter;
                contextService.insertIntoContext('previousFilters', previousFilters, false);
            };

            
            function fetchPreviousFilter(application, schemaId) {
                const applicationKey = application + "." + schemaId;
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