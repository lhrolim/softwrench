(function (mobileServices) {
    "use strict";

    mobileServices.config(["$httpProvider", function ($httpProvider) {

        $httpProvider.defaults.withCredentials = true;

        var ajaxInterceptor = function ($q, $rootScope, $timeout, contextService, $log, $injector) {

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
                if (rejection.status === 401) {
                    // getting around circular dependency problem ($state -> $http -> ... -> securityService -> routeService -> $state)
                    if(!securityService) securityService = $injector.get("securityService");
                    securityService.handleForbiddenStatus();
                    return;
                }
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
                    endederror(rejection);
                    return $q.reject(rejection);
                }
            };

            return interceptor;
        };

        $httpProvider.interceptors.push(["$q", "$rootScope", "$timeout", "contextService", "$log", "$injector", ajaxInterceptor]);

    }]);

})(mobileServices);



