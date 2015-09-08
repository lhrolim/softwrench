(function (mobileServices) {
    "use strict";

    mobileServices.config(["$httpProvider", function ($httpProvider) {

        $httpProvider.defaults.withCredentials = true;

        var ajaxInterceptor = function ($q, $rootScope, $timeout, contextService, $log, networkConnectionService, $injector) {

            var securityService = null;

            var started = function (config) {
                config.headers['offlineMode'] = true;
                var log = $log.getInstance('sw4.ajaxint#started');
                log.debug("url: {0} ".format(config.url));
            };

            var endedok = function (response) {
                var log = $log.getInstance('sw4.ajaxint#endedok');
                log.debug("status :{0}, url: {1} ".format(response.status, response.config.url));
            };

            var endederror = function (rejection) {
                var status = rejection.status;
                if (status === 0) {
                    // connection problem
                    if (networkConnectionService.isOffline()) {
                        // no connection at all
                        return new Error("No internet connection detected.");
                    } 
                    // request-response timeout or server unreachable/socket timeout 
                    return new Error((!rejection.config.timeout ? "Server unreachable" : "Request timed out") + ". Please check your internet connection.");
                
                } else if (status >= 500 && status < 600) {
                    // internal server error
                    return new Error("Internal server error. Please contact support.");

                } else if (status === 404) {
                    // resource not found
                    return new Error("Requested resource not found. Please contact support.");

                } else if (status === 401) {
                    // unauthorized access
                    if (!securityService) securityService = $injector.get("securityService"); // getting around circular dependency problem ($state -> $http -> ... -> securityService -> routeService -> $state)
                    securityService.handleUnauthorizedRemoteAccess();
                } 
                return rejection;

            };

            var interceptor = {
                // optional method
                'request': function (config) {
                    started(config);
                    return config || $q.when(config);
                },
                // optional method
                'response': function (response) {
                    endedok(response);
                    return response || $q.when(response);
                },
                // optional method
                'responseError': function (rejection) {
                    var error = endederror(rejection);
                    return $q.reject(error || rejection);
                }
            };

            return interceptor;
        };

        $httpProvider.interceptors.push(["$q", "$rootScope", "$timeout", "contextService", "$log", "networkConnectionService", "$injector", ajaxInterceptor]);

    }]);

})(mobileServices);



