﻿var app = angular.module('sw_layout');

app.factory('userPreferenceService', function (contextService, restService, $log, $rootScope) {

    function doLoadFilter(shared, application, schema) {
        var user = contextService.getUserData();
        var preferences = user.userPreferences;
        var filters = preferences.gridFilters;
        var result = [];
        var log = $log.getInstance("#userpreferenceservice#doLoadFilter");
        $.each(filters, function (key, association) {
            if (association.filter.application.equalIc(application)
                && association.filter.schema.equalIc(schema)
                && ((association.filter.creatorId == user.dbId) ^ shared)) {
                log.debug("loading filter {0}; shared?".format(association.filter.alias, shared));
                result.push(association.filter);
            }
        });
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
            var preferences = user.userPreferences;
            var associations = preferences.gridFilters;
            var filters = $.grep(associations, function (e) {
                return e.filter.application == application && e.filter.schema == schema;
            });
            return filters.length > 0;
        },

        saveFilter: function (schema, searchData, searchOperators, alias, id, filterowner, successCbk) {
            var fields = "";
            var operators = "";
            var values = "";
            var user = contextService.getUserData();
            var log = $log.getInstance("#userpreferenceservice#savefilter");
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
                alias: alias,
                id: id
            };

            if (id == null || filterowner != user.dbId) {
                log.debug("creating new filter {0}".format(JSON.stringify(parameters)));
                restService.invokePost("GridFilter", "CreateNewFilter", parameters, null, function (resultdata) {
                    var filterassociation = resultdata.resultObject;
                    var preferences = user.userPreferences;
                    var filters = preferences.gridFilters;
                    filters.push(filterassociation);
                    successCbk(filterassociation.filter);
                });
            } else {
                log.debug("updating filter {0}".format(JSON.stringify(parameters)));
                restService.invokePost("GridFilter", "UpdateFilter", parameters, null, function (resultdata) {
                    var preferences = user.userPreferences;
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
            var log = $log.getInstance("#userpreferenceservice#deletefilter");
            var user = contextService.getUserData();
            restService.invokePost("GridFilter", "DeleteFilter", parameters, null, function (resultdata) {
                var preferences = user.userPreferences;
                var association = resultdata.resultObject;
                log.debug("removing filter {0}".format(association.filter.alias));
                preferences.gridFilters = $.grep(preferences.gridFilters, function (value) {
                    return value.id != association.id;
                });
                cbk();
            });
        }


    };

});


