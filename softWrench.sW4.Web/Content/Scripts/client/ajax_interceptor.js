(function (angular, $) {
    "use strict";

angular.module('sw_layout').config(['$httpProvider', function ($httpProvider) {

    $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log, logoutService, schemaCacheService) {
        var activeRequests = 0;
        var started = function (config) {
            lockCommandBars();
            lockTabs();
            config.headers['currentmodule'] = contextService.retrieveFromContext('currentmodule');
            config.headers['currentmetadata'] = contextService.retrieveFromContext('currentmetadata');
            config.headers['mockerror'] = sessionStorage['mockerror'];
            config.headers['requesttime'] = new Date().getTime();
            config.headers['cachedschemas'] = schemaCacheService.getSchemaCacheKeys();
            config.headers['printmode'] = config.printMode;
            var log = $log.getInstance('sw4.ajaxint#started');
            var spinAvoided = false;
            if (activeRequests <= 0 || config.url.indexOf("/Content/") < 0) {
                if (!log.isLevelEnabled('trace')) {
                    log.info("started request {0}".format(config.url));
                }
                spinAvoided = config.avoidspin;
                if (!spinAvoided) {
                    activeRequests++;
                    $rootScope.$broadcast('sw_ajaxinit');
                }
            }
            log.trace("url: {0} | current module:{1} | current metadata:{2} "
               .format(config.url, config.headers['currentmodule'], config.headers['currentmetadata']));
            activeRequests++;
        };
        var endedok = function (response) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
            $('[rel=tooltip]').tooltip('hide');
            activeRequests--;
            var log = $log.getInstance('sw4.ajaxint#endedok');
            log.trace("status :{0}, url: {1} ".format(response.status, response.config.url));
            var spinAvoided = false;
            spinAvoided = response.config.avoidspin;
            if (!spinAvoided) {
                activeRequests--;
            }
            if (activeRequests <= 0) {
                unLockCommandBars();
                unLockTabs();
                log.info("Requests ended");
                $rootScope.$broadcast('sw_ajaxend', response.data);
                successMessageHandler(response.data);
            }
        };

        function successMessageHandler(data) {
            var timeOut = contextService.retrieveFromContext('successMessageTimeOut');
            if (data.successMessage != null) {
                var willRefresh = contextService.fetchFromContext("refreshscreen", false, true);
                if (!willRefresh) {
                    //if we´re sure the page will refresh we shall not display the successmessage now, since it would disappear quickly
                    $rootScope.$broadcast('sw_successmessage', data);
                    $timeout(function() {
                        $rootScope.$broadcast('sw_successmessagetimeout', { successMessage: null });
                    }, timeOut);
                } else {
                    contextService.insertIntoContext("onloadMessage", data.successMessage);
                    contextService.deleteFromContext("refreshscreen");
                }
                
            }
        }

        var endederror = function (rejection) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
            $('[rel=tooltip]').tooltip('hide');
            

            if (rejection.status === 401) {
                window.location = url('');
                return;
            }
            activeRequests--;
            unLockCommandBars();
            unLockTabs();
            if (activeRequests <= 0) {
                $rootScope.$broadcast('sw_ajaxerror', rejection.data,rejection.status);
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

    $httpProvider.defaults.transformRequest.push(function (data, headers) {
        if (data == undefined) {
            return data;
        }
        if (sessionStorage.mockerror || sessionStorage.mockmaximo) {
            var jsonOb = JSON.parse(data);
            jsonOb['%%mockerror'] = sessionStorage.mockerror === "true";
            jsonOb['%%mockmaximo'] = sessionStorage.mockmaximo === "true";
            return JSON.stringify(jsonOb);
        }
        return data;
    });
}]);


window.onpopstate = function (e) {
    if (e.state) {
        document.getElementById("content").innerHTML = e.state.html;
        document.title = e.state.pageTitle;
    }
};

})(angular, jQuery);