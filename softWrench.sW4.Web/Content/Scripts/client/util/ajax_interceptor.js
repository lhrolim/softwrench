(function (angular) {
    "use strict";
    angular.module('sw_layout')
        .config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log, schemaCacheService, crudContextHolderService, alertService) {
                var activeRequests = 0;
                var activeRequestsArr = [];
                var started = function (config) {
                    const spinAvoided = config.avoidspin || false;
                    if (!spinAvoided) {
                        lockCommandBars();
                        lockTabs();
                    }
                    const panelid = config.headers['panelid'] || null;

                    config.headers['currentmodule'] = config.headers['currentmodule'] || contextService.retrieveFromContext('currentmodule');
                    config.headers['currentprofile'] = config.headers['currentprofile'] || crudContextHolderService.getCurrentSelectedProfile(panelid);
                    config.headers['constrainedprofiles'] = config.headers['constrainedprofiles'] || crudContextHolderService.getConstrainedProfiles(panelid);
                    config.headers['currentmetadata'] = config.headers['currentmetadata'] || contextService.retrieveFromContext('currentmetadata');
                    config.headers['mockerror'] = sessionStorage['mockerror'];
                    config.headers['mockmaximo'] = sessionStorage['mockmaximo'];
                    config.headers['mocksecurity'] = sessionStorage['mocksecurity'];
                    config.headers['ignoreconfigcache'] = sessionStorage['ignoreconfigcache'];
                    config.headers['requesttime'] = new Date().getTime();
                    config.headers["isajax"] = true;
                    config.headers['cachedschemas'] = schemaCacheService.getSchemaCacheKeys();
                    const log = $log.getInstance('sw4.ajaxint#started');
                    if (config.url.indexOf("/Content/") < 0) {
                        //let´s ignore angularjs templates loading, that would pass through here as well
                        if (!log.isLevelEnabled('trace')) {
                            log.info("started request {0}".format(config.url));
                        }

                        if (!spinAvoided) {
                            activeRequests++;
                            $rootScope.$broadcast(JavascriptEventConstants.AjaxInit);
                            activeRequestsArr.push(config.url);
                        }
                    }
                    log.trace("url: {0} | current module:{1} | current metadata:{2} | spin avoided:{3} "
                       .format(config.url, config.headers['currentmodule'], config.headers['currentmetadata'], spinAvoided));

                };
                var endedok = function (response) {
                    //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
                    $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                    $('.no-touch [rel=tooltip]').tooltip('hide');
                    const spinAvoided = response.config.avoidspin;
                    if (!spinAvoided) {
                        activeRequests--;
                    }
                    const log = $log.getInstance('sw4.ajaxint#endedok');
                    log.trace("status :{0}, url: {1} ".format(response.status, response.config.url));
                    if (activeRequests <= 0) {
                        activeRequests = 0;
                        if (!spinAvoided) {
                            unLockCommandBars();
                            unLockTabs();
                        }


                        log.info("Requests ended");
                        $rootScope.$broadcast(JavascriptEventConstants.AjaxFinished, response.data);
                        successMessageHandler(response.data);
                        //this has to be renewed per operation
                        contextService.insertIntoContext("avoidspin", null, true);
                    }
                    const idx = activeRequestsArr.indexOf(response.config.url);
                    if (idx >= 0) {
                        activeRequestsArr.splice(idx, 1);
                    }
                };

                function successMessageHandler(data) {
                    if (!!data && !!data.successMessage) {
                        const willRefresh = contextService.fetchFromContext("refreshscreen", false, true);
                        if (!willRefresh) {
                            //use $timeout to make sure the notification timing works correctly
                            $timeout(function () {
                                if (data.warningDto) {
                                    alertService.notifyWarning(data);
                                } else {
                                    alertService.notifymessage('success', data.successMessage);
                                }
                            }, 0);

                        } else {
                            contextService.insertIntoContext("onloadMessage", data.successMessage);
                            contextService.deleteFromContext("refreshscreen");
                        }
                    }
                }

                var endederror = function (rejection) {
                    //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
                    $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
                    $('.no-touch [rel=tooltip]').tooltip('hide');

                    if (rejection.status === 401) {
                        window.location = url('');
                        return;
                    }
                    activeRequests--;
                    unLockCommandBars();
                    unLockTabs();
                    //            if (activeRequests <= 0) {
                    $rootScope.$broadcast(JavascriptEventConstants.ErrorAjax, rejection.data);
                    if (rejection.data && rejection.data.notifyException !== false) {
                        alertService.notifyexception(rejection.data);
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
                    const jsonOb = angular.fromJson(data);
                    const objtoSet = jsonOb.json ? jsonOb.json : jsonOb;
                    objtoSet['%%mockerror'] = sessionStorage.mockerror === "true";
                    objtoSet['%%mockmaximo'] = sessionStorage.mockmaximo === "true";
                    return angular.toJson(jsonOb);
                }
                return data;
            });

            $httpProvider.defaults.transformResponse.push(
                // response transformer to maintain compatibility
                function (data, headers) {
                    if (angular.isUndefined(data)) {
                        return angular.toJson({});
                    } else if (data === null) {
                        return "null";
                    }
                    return data;
                });

        }]);
})(angular);


window.onpopstate = function (e) {
    if (e.state) {
        document.getElementById("content").innerHTML = e.state.html;
        document.title = e.state.pageTitle;
    }
};
