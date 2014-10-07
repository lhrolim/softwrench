var app = angular.module('sw_layout');

app.factory('userPreferenceService', function (contextService, restService) {

    function doLoadFilter(shared) {
        var user = contextService.getUserData();
        var preferences = user.preferences;
        var filters = preferences.gridFilters;
        var result = [];
        $.each(filters, function (key, association) {
            if (association.creator ^ shared) {
                result.push(association.filter);
                return;
            }
        });
        return result;
    }

    return {

        loadUserSharedFilters: function () {
            return doLoadFilter(true);
        },

        loadUserNonSharedFilters: function () {
            return doLoadFilter(false);
        },

        saveFilter: function (application, schema, querystring, alias) {
            var parameters = {
                application: application,
                schema: schema,
                querystring: querystring,
                alias: alias
            };
            restService.invokePost("GridFilter", "CreateNewFilter", parameters);
        }


    };

});


