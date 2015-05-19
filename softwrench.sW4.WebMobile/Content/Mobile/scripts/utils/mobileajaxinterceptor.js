modules.webcommons.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log) {
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
            if (rejection.status == 401) {
                window.location = url('');
                return;
            }
        };

        return {
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
    });


}]);


