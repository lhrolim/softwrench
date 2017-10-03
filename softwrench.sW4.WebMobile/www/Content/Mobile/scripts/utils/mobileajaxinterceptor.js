(function (angular, mobileServices) {
    "use strict";

    mobileServices.config(["$httpProvider", function ($httpProvider) {

        $httpProvider.defaults.withCredentials = true;
        $httpProvider.defaults.headers.common["offlineMode"] = true;

        function ajaxInterceptor($q, $rootScope, $timeout, contextService, $log, networkConnectionService, $injector) {

            var securityService = null;

            const started = function (config) {
                config.headers["offlineMode"] = true;
                config.headers["request_start_timestamp"] = new Date().getTime();
                const log = $log.getInstance("sw4.ajaxint#started");
                log.debug(`url: ${config.url}`);
            };

            const endedok = function (response) {
                const log = $log.getInstance("sw4.ajaxint#endedok");
                log.debug(`status: ${response.status}, url: ${response.config.url}`);
            };

            function isTimeoutError(rejection) {
                const timeout = rejection.config.timeout;
                const start = rejection.config["request_start_timestamp"];
                const now = new Date().getTime();
                return angular.isNumber(timeout) // timeout defined
                    && angular.isNumber(start) // start timestamp defined
                    && ((now - start) > timeout); // timeout has passed
            }

            const endederror = function (rejection) {
                const status = rejection.status;
                if (status === 0 || status === -1) {
                    // connection problem
                    if (networkConnectionService.isOffline()) {
                        // no connection at all
                        return new Error("No internet connection detected.");
                    }
                    const timeout = isTimeoutError(rejection);
                    // request-response timeout or server unreachable/socket timeout 
                    const message = (timeout ? "Request timed out" : "Server unreachable") + ". Please make sure the url in settings is correct and check your internet connection.";
                    const error = new Error(message);
                    if (rejection.config.url.contains("SignIn/Ping")) {
                        //to avoid circular error handling for ping calls that are made in order to double check server reacheability
                        return rejection;
                    }
                    error.type = timeout ? "timeout" : "unreacheable";    
                    
                    return error;
                
                } else if (status >= 500 && status < 600) {
                    const stack = rejection.data ? rejection.data.fullStack : "";
                    const error = new Error("Internal server error. Please contact support.");
                    if (!!stack) {
                        error.stack = stack;
                    }
                    // internal server error
                    return error;

                } else if (status === 404) {
                    // resource not found
                    return new Error("Requested resource not found. Please contact support.");

                } else if (status === 401) {
                    // unauthorized access
                    if (!securityService) securityService = $injector.get("securityService"); // getting around circular dependency problem ($state -> $http -> ... -> securityService -> routeService -> $state)
                    securityService.handleUnauthorizedRemoteAccess(rejection);
                } 
                return rejection;
            };

            const interceptor = {
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
                    const error = endederror(rejection);
                    return $q.reject(error || rejection);
                }
            };
            return interceptor;
        };
        
        $httpProvider.interceptors.push(["$q", "$rootScope", "$timeout", "contextService", "$log", "networkConnectionService", "$injector", ajaxInterceptor]);

    }]);

})(angular, mobileServices);