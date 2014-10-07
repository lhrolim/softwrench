var app = angular.module('sw_layout');

app.factory('restService', function ($http) {


    return {

        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            var params = parameters == null ? {} : parameters;
            return url("/api/generic/" + controller + "/" + action + "?" + $.param(params));
        },

        invokePost: function (controller, action, queryParameters, json, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            $http.post(url, json)
                .success(function (data) {
                    if (successCbk != null) {
                        successCbk(data);
                    }
                })
                .error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
        }


    };

});


