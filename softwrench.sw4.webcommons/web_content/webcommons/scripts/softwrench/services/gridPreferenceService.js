(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('gridPreferenceService', function (contextService, restService, $log, searchService, previousFilterService) {
    "ngInject";

    function compareFilters(a, b) {
        if (a.alias < b.alias) {
            return -1;
        }
        if (a.alias > b.alias) {
            return 1;
        }
        return 0;
    }

    function doLoadFilter(shared, application, schema) {
        var user = contextService.getUserData();
        var gridpreferences = user.gridPreferences;
        var result = [];
        if (!gridpreferences) {
            return result;
        }

        var filters = gridpreferences.gridFilters;
        
        var log = $log.getInstance("#gridpreferenceservice#doLoadFilter");
        $.each(filters, function (key, association) {
            if (association.filter.application.equalIc(application)
                && association.filter.schema.equalIc(schema)
                && ((association.filter.creatorId == user.dbId) ^ shared)) {
                log.debug("loading filter {0}; shared?".format(association.filter.alias, shared));
                result.push(association.filter);
            }
        });
        result = result.sort(compareFilters);

        var applicationKey = application + '.' + schema;
        var previousFilter = previousFilterService.fetchPreviousFilter(applicationKey);
        if (previousFilter != null && previousFilter !== {}) {
            log.debug("Adding previous filter for schema {0}".format(schema));
            result.push(previousFilter);
        }
        return result;
    }

    return {

        loadUserSharedFilters: function (application, schema) {
            return doLoadFilter(true, application, schema);
        },

        loadUserNonSharedFilters: function (application, schema) {
            return doLoadFilter(false, application, schema);
        },

        hasFilter: function (application, schema) {
            var user = contextService.getUserData();
            var preferences = user.gridPreferences;
            var associations = preferences.gridFilters;
            var filters = $.grep(associations, function (e) {
                return e.filter.application == application && e.filter.schema == schema;
            });
            return filters.length > 0;
        },

        saveFilter: function (schema, searchData, template, searchOperators, searchSort, advancedSearch, alias, id, filterowner, successCbk) {
            var fields = "";
            var operators = "";
            var values = "";
            var user = contextService.getUserData();
            var log = $log.getInstance("#gridpreferenceservice#savefilter");
            for (var data in searchData) {
                if (data == "lastSearchedValues") {
                    continue;
                }
                fields += data + ",";
                values += searchData[data] + ",,,";
                var operator = searchOperators[data];
                // the operators are only setted if the user specific select them, but they should be 'C' as default --> avoiding inconsistency
                operators += (operator == null ? 'C' : operator.symbol) + ",";
            }


            var parameters = {
                application: schema.applicationName,
                schema: schema.schemaId,
                fields: fields.substr(0, fields.length - 1),
                operators: operators.substr(0, operators.length - 1),
                values: values.substr(0, values.length - 3),
                template: template,
                alias: alias,
                advancedSearch: advancedSearch,
                id: id
            };

            if (id == null || filterowner != user.dbId) {
                log.debug("creating new filter {0}".format(JSON.stringify(parameters)));
                restService.invokePost("GridFilter", "CreateNewFilter", parameters, null, function (resultdata) {
                    var filterassociation = resultdata.resultObject;
                    var preferences = user.gridPreferences;
                    var filters = preferences.gridFilters;
                    filters.push(filterassociation);
                    successCbk(filterassociation.filter);
                });
            } else {
                log.debug("updating filter {0}".format(JSON.stringify(parameters)));
                restService.invokePost("GridFilter", "UpdateFilter", parameters, null, function (resultdata) {
                    var preferences = user.gridPreferences;
                    var associations = preferences.gridFilters;
                    var filter = resultdata.resultObject;
                    for (var i = 0; i < associations.length; i++) {
                        if (associations[i].filter.id == filter.id) {
                            associations[i].filter = filter;
                        }
                    }
                    successCbk(filter);
                });
            }
        },

        deleteFilter: function (filterId, creatorId, cbk) {
            var parameters = {
                filterId: filterId,
                creatorId: creatorId,
            };
            var log = $log.getInstance("#gridpreferenceservice#deletefilter");
            var user = contextService.getUserData();
            restService.invokePost("GridFilter", "DeleteFilter", parameters, null, function (resultdata) {
                var gridpreferences = user.gridPreferences;
                var association = resultdata.resultObject;
                log.debug("removing filter {0}".format(association.filter.alias));
                gridpreferences.gridFilters = $.grep(gridpreferences.gridFilters, function (value) {
                    return value.id != association.id;
                });
                cbk();
            });
        },

        applyFilter: function (filter, searchOperator, quickSearchDTO, panelid) {
            var searchData = {};
            if (filter.fields) {
                var fieldsArray = filter.fields.split(",");
                var operatorsArray = filter.operators.split(",");
                var valuesArray = filter.values.split(",,,");
                for (var i = 0; i < fieldsArray.length; i++) {
                    var field = fieldsArray[i];
                    searchData[field] = valuesArray[i];
                    searchOperator[field] = searchService.getSearchOperationBySymbol(operatorsArray[i]);
                }
            } else {
                searchOperator = {};
            }
            
            var template = filter.template;
            var searchSort = filter.searchSort || {};

            searchService.refreshGrid(searchData, searchOperator, { searchTemplate: template, quickSearchDTO: quickSearchDTO, panelid: panelid, searchSort: searchSort });
        }
    };

});

})(angular);