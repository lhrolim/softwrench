﻿angular.module('sw_layout').factory('restService', function ($http, $log, contextService) {
    "ngInject";

    return {




        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            var params = parameters == null ? {} : parameters;
            var serverUrl = contextService.getFromContext("serverurl");
            if (serverUrl) {
                return serverUrl + "/api/generic/" + controller + "/" + action + "?" + $.param(params, true);
            }
            return url("/api/generic/" + controller + "/" + action + "?" + $.param(params, true));
        },

        invokePost: function (controller, action, queryParameters, json, successCbk, failureCbk) {
            this.postPromise(controller, action, queryParameters, json)
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
        },

        postPromise: function (controller, action, queryParameters, json) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokePost");
            log.info("invoking post on url {0}".format(url));
            return $http.post(url, json);
        },

        getPromise: function (controller, action, queryParameters) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            return $http.get(url);
        },

        invokeGet: function (controller, action, queryParameters, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            var getPromise = this.getPromise(controller, action, queryParameters);
            getPromise
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


