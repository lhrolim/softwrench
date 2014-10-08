var app = angular.module('sw_layout');

app.factory('userPreferenceService', function (contextService, restService) {

    function doLoadFilter(shared, application, schema) {
        var user = contextService.getUserData();
        var preferences = user.userPreferences;
        var filters = preferences.gridFilters;
        var result = [];
        $.each(filters, function (key, association) {
            if (association.filter.application.equalIc(application)
                && association.filter.schema.equalIc(schema)
                && (association.creator ^ shared)) {
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
            var filters = preferences.gridFilters;
            return filters.length > 0;
        },

        saveFilter: function (application, schema, searchData, searchOperators, alias) {
            var fields="";
            var operators = "";
            var values = "";
            for (var data in searchData) {
                if (data == "lastSearchedValues") {
                    continue;
                }
                fields += data + ",";
                values += searchData[data] + ",,,";
            }

            for (var operator in searchOperators) {
                operators += searchOperators[operator].symbol + ",";
            }

            var parameters = {
                application: application,
                schema: schema,
                fields: fields.substr(0,fields.length-1),
                operators: operators.substr(0, operators.length - 1),
                values: values.substr(0, values.length - 3),
                alias: alias
            };
            restService.invokePost("GridFilter", "CreateNewFilter", parameters);
        }


    };

});


