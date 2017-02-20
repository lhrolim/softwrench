(function (modules) {
    "use strict";

modules.webcommons.service('restService', ["$http", "$log", "contextService", function ($http, $log, contextService) {

    return {

        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            var params = parameters == null ? {} : parameters;
            var serverUrl = contextService.getFromContext("serverurl");
            if (serverUrl) {
                return serverUrl + "/api/generic/" + controller + "/" + action + "?" + $.param(params, false);
            }
            return url("/api/generic/" + controller + "/" + action + "?" + $.param(params, false));
        },
        /**
         * Deprecated: use postPromise instead
         * @deprecated 
         */
        invokePost: function (controller, action, queryParameters, json, successCbk, failureCbk) {
            this.postPromise(controller, action, queryParameters, json)
                .then(function (response) {
                    if (successCbk != null) {
                        successCbk(response.data);
                    }
                })
                .catch(function (response) {
                    if (failureCbk != null) {
                        failureCbk(response.data);
                    }
                });
        },
        /**
         * Deprecated: use getPromise instead
         * @deprecated
         */
        invokeGet: function (controller, action, queryParameters, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            var getPromise = this.getPromise(controller, action, queryParameters);
            getPromise
                .then(function (response) {
                    if (successCbk != null) {
                        successCbk(response.data);
                    }
                })
                .catch(function (response) {
                    if (failureCbk != null) {
                        failureCbk(response.data);
                    }
                });
        },

        /**
         * Sends a POST request to an ASP.NET Controller. 
         * 
         * @param String controller ASP.NET Controller name 
         * @param String action ASP.NET Controller's action method's name
         * @param Object queryParameters dictionary of parameters to be passed as query string
         * @param Object json request's payload
         * @param Object config request's config
         * 
         * @returns HttpPromise
         */
        postPromise: function (controller, action, queryParameters, json, config) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokePost",["post","network"]);
            log.info("invoking post on url {0}".format(url));
            return $http.post(url, json, config);
        },

        post: function () {
            return this.postPromise.apply(this, arguments);
        },

        /**
         * Sends a GET request to an ASP.NET Controller. 
         * 
         * @param String controller ASP.NET Controller name 
         * @param String action ASP.NET Controller's action method's name
         * @param Object queryParameters dictionary of parameters to be passed as query string
         * @param Object config request's config
         * @returns HttpPromise
         */
        getPromise: function (controller, action, queryParameters, config) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            return $http.get(url, config);
        },

        getPromiseNoDigest: function (controller, action, queryParameters, config = {}) {
             var url = this.getActionUrl(controller, action, queryParameters);
             var log = $log.getInstance("restService#invokeGet");
             log.info("invoking get on url {0}".format(url));
             return $http.get("nodigest:"+url, config);
        },

        get: function() {
            return this.getPromise.apply(this, arguments);
        }

    };

}]);

})(modules);
