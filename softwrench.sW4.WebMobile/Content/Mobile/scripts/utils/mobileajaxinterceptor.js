modules.webcommons.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log) {
        var started = function (config) {
            config.headers['offlineMode'] = true;
            var log = $log.getInstance('sw4.ajaxint#started');
            log.trace("url: {0} | current module:{1} | current metadata:{2} "
               .format(config.url, config.headers['currentmodule'], config.headers['currentmetadata']));
        };

        var endedok = function (response) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
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


