var app = angular.module('sw_layout');


(function (angular) {
    'use strict';


    function redirectService($http, $rootScope, $log, $q, contextService, fixHeaderService, restService, applicationService, alertService, $timeout) {


        function getActionUrl(controller, action, parameters) {
            return restService.getActionUrl(controller, action, parameters);
        };

        function redirectToHome() {
            contextService.deleteFromContext("swGlobalRedirectURL");
            window.location.reload();
        };

        function redirectToAction(title, controller, action, parameters, target) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            if (title != null) {
                parameters.title = title;
            }
            var redirectUrl;
            if (target == 'new') {
                redirectUrl = url(controller + "/" + action + "?" + $.param(parameters));
                var w = window.open(redirectUrl);
                w.moveTo(0, 0);
                return $q.when();
            }

            redirectUrl = restService.getActionUrl(controller, action, parameters);

            contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
            return $http.get(redirectUrl).success(
                function (data) {
                    if (data.type != "BlankApplicationResponse") {
                        $rootScope.$broadcast("sw_redirectactionsuccess", data);
                    }
                }).error(
                function (data) {
                    var errordata = {
                        errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                        errorStack: data.message
                    }
                    $rootScope.$broadcast("sw_ajaxerror", errordata);
                    alertService.notifyexception(errordata);
                });
        };

        function getApplicationUrl(applicationName, schemaId, mode, title, parameters) {
            return applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters);
        };

        function redirectToTab(tabId) {
            $timeout(function () {
                //this timeout is needed because a digest might already be in progress
                contextService.setActiveTab(tabId);
                var tab = $('a[href="#' + tabId + '"]');
                tab.trigger('click');;
            }, 0, false);
        };

        function goToAction(controller, action, parameters) {
            /// <summary>
            /// Shortcut method for the most common case
            /// </summary>
            /// <param name="title"></param>
            /// <param name="controller"></param>
            /// <param name="action"></param>
            /// <param name="parameters"></param>
            /// <param name="target"></param>
            return this.redirectToAction(null, controller, action, parameters, null);
        };

        function goToApplication(applicationName, schemaId, parameters, jsonData) {
            this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData);
        };

        function redirectWithData(applicationName, schemaId, mode, searchData, extraParameters) {
            /// <summary>
            /// Shortcut method to redirect to an application with search data 
            /// </summary>

            if (!extraParameters) {
                //to avoid extra checkings all along
                extraParameters = {};
            }
            if (!searchData) {
                searchData = {};
            }
            var log = $log.getInstance('redirectService#redirectWithData');

            var searchDTO = this.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = extraParameters.pageNumber ? extraParameters.pageNumber : 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = extraParameters.pageSize ? extraParameters.pageSize : 30;

            var restParameters = {
                key: {
                    schemaId: schema ? schema : "list",
                    mode: extraParameters.mode ? extraParameters.mode : 'none',
                    platform: "web"
                },
                SearchDTO: searchDTO
            };
            var queryString = $.param(restParameters);
            var urlToUse = url("/api/Data/{0}?{1}".format(application, queryString));
            log.info("invoking url {0}".format(urlToUse));
            var jsonData = {};
            $http.get(urlToUse).success(function (data) {
                jsonData = data;
                this.goToApplicationView(applicationName, schemaId, null, null, null, jsonData);
            }).
                error(function (data) {

                });
        };

        function goToApplicationView(applicationName, schemaId, mode, title, parameters, jsonData, afterRedirectHook, type) {
            var log = $log.getInstance('redirectService#goToApplication', ["redirect"]);

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData, type);

            var popupMode = parameters.popupmode;

            if (popupMode === "browser") {
                if (contextService.isLocal()) {
                    //easier to debug on chrome like this
                    var w = window.open(redirectURL);
                    //                    w.moveto(0, 0);
                } else {
                    var x = screen.width / 2 - 800 / 2;
                    var y = screen.height / 2 - 600 / 2;
                    var w = window.open(redirectURL, '_blank', 'height=600px,width=800px,left=' + x + ',top=' + y + ',resizable=yes,scrollbars=yes', false);
                    w.focus();
                }
                return;
            }

            //this code will get called when the user is already on a crud page and tries to switch view only.
            $rootScope.popupmode = popupMode;
            fixHeaderService.unfix();



            if (jsonData == undefined) {


                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                return $http.get(redirectURL).then(function (data) {
                    contextService.insertIntoContext("swGlobalRedirectURL", redirectURL, false);
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data.data, mode, applicationName);
                    if (afterRedirectHook != null) {
                        afterRedirectHook();
                    }
                    return $q.when(data.data.resultObject);
                });
            } else {
                var jsonString = angular.toJson(jsonData);
                log.info('invoking post on datacontroller for {0} | content: '.format(applicationName, jsonString));
                return $http.post(redirectURL, jsonString).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data.data, mode, applicationName);
                    return $q.when(data.data.resultObject);
                });
            }
        };

        function redirectNewWindow(newWindowURL, needReload, initialData) {

            if (contextService.isLocal()) {
                //easier to debug on chrome like this
                var w = window.open(newWindowURL);
                w.moveTo(0, 0);
                return;
            }

            var cbk = function (view) {
                var x = window.open('', '_blank', 'height=600px,width=800px,left=350px,top=100px,resizable=yes,scrollbars=yes', false);

                x.document.open();
                x.document.write(view);
                x.document.close();

                if (needReload == true) {
                    x.location.reload();
                }
                x.focus();
                if (contextService.isLocal()) {
                    x.moveTo(0, 0);
                }
            };
            if (initialData == undefined) {
                $http.post(newWindowURL).success(cbk);
            } else {
                var jsonString = angular.toJson(initialData);
                $http.post(newWindowURL, jsonString).success(cbk);
            }

        }


        var service = {
            getActionUrl: getActionUrl,
            getApplicationUrl: getApplicationUrl,
            goToAction: goToAction,
            goToApplication: goToApplication,
            goToApplicationView: goToApplicationView,
            redirectNewWindow: redirectNewWindow,
            redirectToAction: redirectToAction,
            redirectToHome: redirectToHome,
            redirectToTab: redirectToTab,
            redirectWithData: redirectWithData
        };

        return service;
    }

    angular
    .module('sw_layout')
    .factory('redirectService', ['$http', '$rootScope', '$log', '$q', 'contextService', 'fixHeaderService', 'restService', 'applicationService', 'alertService', '$timeout', redirectService]);


})(angular);

