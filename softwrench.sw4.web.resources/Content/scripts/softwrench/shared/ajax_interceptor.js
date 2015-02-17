var app = angular.module('sw_layout');

app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log) {
        var activeRequests = 0;
        var started = function (config) {
            lockCommandBars();
            lockTabs();
            config.headers['currentmodule'] = contextService.retrieveFromContext('currentmodule');
            config.headers['currentmetadata'] = contextService.retrieveFromContext('currentmetadata');
            config.headers['mockerror'] = sessionStorage['mockerror'];
            var log = $log.getInstance('sw4.ajaxint#started');
            if (config.url.indexOf("/Content/") == -1) {
                //let´s ignore angularjs templates loading, that would pass through here as well
                if (!log.isLevelEnabled('trace')) {
                    log.info("started request {0}".format(config.url));
                }
                if (!$rootScope.avoidspin) {
                    $rootScope.$broadcast('sw_ajaxinit');
                }
            }
            log.trace("url: {0} | current module:{1} | current metadata:{2} "
               .format(config.url, config.headers['currentmodule'], config.headers['currentmetadata']));
            activeRequests++;

        };
        var endedok = function (response) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
            $('.no-touch [rel=tooltip]').tooltip('hide');
            activeRequests--;
            var log = $log.getInstance('sw4.ajaxint#endedok');
            log.trace("status :{0}, url: {1} ".format(response.status, response.config.url));
            if (activeRequests == 0) {
                unLockCommandBars();
                unLockTabs();
                log.info("Requests ended");
                $rootScope.$broadcast('sw_ajaxend', response.data);
                successMessageHandler(response.data);
                //this has to be renewed per operation
                contextService.insertIntoContext("avoidspin", null, true);
            }
        };

        function successMessageHandler(data) {
            var timeOut = contextService.retrieveFromContext('successMessageTimeOut');
            if (data.successMessage != null) {
                var willRefresh = contextService.fetchFromContext("refreshscreen", false, true);
                if (!willRefresh) {
                    $rootScope.$broadcast('sw_successmessage', data);
                    $timeout(function () {
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
            $('.no-touch [rel=tooltip]').tooltip('hide');
            if (rejection.status == 401) {
                window.location = url('');
                return;
            }
            activeRequests--;
            unLockCommandBars();
            unLockTabs();
            if (activeRequests == 0) {
                $rootScope.$broadcast('sw_ajaxerror', rejection.data);
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
            if (sessionStorage.mockerror == "true") {
                jsonOb['%%mockerror'] = true;
            }
            if (sessionStorage.mockmaximo == "true") {
                jsonOb['%%mockmaximo'] = true;
            }
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