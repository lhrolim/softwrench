(function (modules) {
    "use strict";

modules.rootCommons.service('restService', ["$http", "$log","$q", "contextService", function ($http, $log,$q, contextService) {

    const anonymousControllers = [
    "Signin",
    "FirstSolarEmail",
    "FirstSolarWpGenericEmail",
    "UmcRequestController",
    "SwgasRequestController",
    "UserSetup/DefinePassword",
    "UserSetup/DoSetPassword",
    "UserSetupWebApi/ForgotPassword",
    "UserSetupWebApi/SendActivationEmail",
    "UserSetupWebApi/NewUserRegistration",
    "FSDBackendController"

    ];

    return {

        
        validateAnonymous: function (controller, action) {
            if (anonymousControllers.some(a=> a === controller)) {
                return true;
            }
            if (anonymousControllers.some(a => a === (controller + "/" + action))) {
                return true;
            }
            return false;
        },

        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            const params = parameters == null ? {} : parameters;
            let serverUrl = contextService.getFromContext("serverurl");
            if (params["_customurl"]) {
                serverUrl = params["_customurl"];
                delete params["_customurl"];
            }
            
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
            const url = this.getActionUrl(controller, action, queryParameters);
            const log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            const getPromise = this.getPromise(controller, action, queryParameters);
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
            const url = this.getActionUrl(controller, action, queryParameters);
            const log = $log.getInstance("restService#invokePost",["post","network"]);
            if (contextService.get("anonymous", false, true) && !this.validateAnonymous(controller, action)) {
                log.warn("mocking call to {0}. consider filtering that request off the stack".format(url));
                return $q.reject();
            } else {
                log.info("invoking post on url {0}".format(url));
                return $http.post(url, json, config);
            }
        },

        post: function () {
            return this.postPromise.apply(this, arguments);
        },

        putPromise: function (controller, action, queryParameters, json, config) {
            const url = this.getActionUrl(controller, action, queryParameters);
            const log = $log.getInstance("restService#invokePut", ["put", "network"]);

            if (contextService.get("anonymous", false, true) && !this.validateAnonymous(controller, action)) {
                log.info("mocking call to {0} consider filtering that request off the stack".format(url));
                return $q.reject();
            } else {
                log.info("invoking put on url {0}".format(url));
                return $http.put(url, json, config);    
            }
        },

        put: function () {
            return this.putPromise.apply(this, arguments);
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
            const url = this.getActionUrl(controller, action, queryParameters);
            const log = $log.getInstance("restService#invokeGet");

            if (contextService.get("anonymous", false, true) && !this.validateAnonymous(controller, action)) {
                log.info("mocking call to {0} consider filtering that request off the stack".format(url));
                return $q.reject();
            } else {
                log.info("invoking get on url {0}".format(url));
                return $http.get(url, config);
            }
        },

        getPromiseNoDigest: function (controller, action, queryParameters, config = {}) {
             const url = this.getActionUrl(controller, action, queryParameters);
             const log = $log.getInstance("restService#invokeGet");
             log.info("invoking get on url {0}".format(url));
             return $http.get("nodigest:"+url, config);
        },

        get: function() {
            return this.getPromise.apply(this, arguments);
        }

    };

}]);

})(modules);
