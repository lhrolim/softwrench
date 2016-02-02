(function (angular) {
    'use strict';


    function redirectService($http, $rootScope, $log, $q, contextService, fixHeaderService, restService, applicationService, alertService,modalService,schemaCacheService, $timeout) {


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

            if (!parameters || !parameters.popupmode) {
                //let´s disregard popupmode here, otherwise F5 would refresh popup 
                contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
            }
            
            return $http.get(redirectUrl).success(
                function (data) {
                    if (data.type !== "BlankApplicationResponse") {
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
            return $timeout(function () {
                //this timeout is needed because a digest might already be in progress
                contextService.setActiveTab(tabId);
                var tab = $('a[href="#' + tabId + '"]');
                tab.trigger('click');
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

        /**
         * Shortcut method
         * @param {type} applicationName
         * @param {type} schemaId
         * @param {type} parameters
         * @param {type} jsonData
         * @returns {type} 
         */
        function openAsModal(applicationName, schemaId, parameters, jsonData) {
            parameters = parameters || {};
            parameters.popupmode = "modal";
            return this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData).then(function(resultObject) {
                return contextService.insertIntoContext("grid_refreshdata", { data: resultObject, panelid: "#modal" }, true);
            });
        };

        /**
         * 
         * @param {type} applicationName name of the application to redirect
         * @param {type} schemaId id of the schema to redirect to
         * @param {type} parameters: any parameters to be passed within the query string plus
         *      popupmode -- modal, browser or null for redirecting the main page;
         *      savefn -- a savefn to be executed on popupmode;
         * @param {type} jsonData a initial data that could be passed to open a schema filled with some values
         */
        function goToApplication(applicationName, schemaId, parameters, jsonData) {
            return this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData);
        };

        function redirectFromServerResponse(serverResponse, mode) {
            mode = mode || "input";
            return $rootScope.$broadcast("sw_redirectapplicationsuccess", serverResponse, mode, serverResponse.applicationName);
        };

        function goToApplicationView(applicationName, schemaId, mode, title, parameters, jsonData, afterRedirectHook, type) {
            var log = $log.getInstance('redirectService#goToApplication', ["redirect"]);
            parameters = parameters || {};
            
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            //let´s exclude savefn from possible parameters, otherwise it would be evaluated by $.param
            var savefn = parameters.savefn;
            parameters.savefn = null;
            var redirectUrl = applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData, type);
            //including back savefn param
            parameters.savefn = savefn;

            var popupMode = parameters.popupmode;

            if (popupMode === "browser") {
                if (contextService.isLocal()) {
                    //easier to debug on chrome like this
                    window.open(redirectUrl);
                    //                    w.moveto(0, 0);
                } else {
                    var x = screen.width / 2 - 800 / 2;
                    var y = screen.height / 2 - 600 / 2;
                    var w = window.open(redirectUrl, '_blank', 'height=600px,width=800px,left=' + x + ',top=' + y + ',resizable=yes,scrollbars=yes', false);
                    w.focus();
                }
                //to keep promise consitent
                return $q.when();
            }

            //this code will get called when the user is already on a crud page and tries to switch view only.
            $rootScope.popupmode = popupMode;
            fixHeaderService.unfix();



            if (jsonData == undefined) {
                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                return $http.get(redirectUrl).then(function (httpResponse) {
                    var data = httpResponse.data;

                    if (popupMode !== "modal") {
                        contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
                        $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                    } else {
                        contextService.insertIntoContext("grid_refreshdata", { data: data.resultObject, panelid: "#modal" }, true);
                        modalService.show(schemaCacheService.getSchemaFromResult(data), data.resultObject.fields,parameters);
                    }
                    if (afterRedirectHook != null) {
                        afterRedirectHook();
                    }
                    return $q.when(data);
                });
            } else {
                var jsonString = angular.toJson(jsonData);
                if (log.isLevelEnabled("info")) {
                    log.info('invoking post on datacontroller for {0} | content: '.format(applicationName, jsonString));
                }
                return $http.post(redirectUrl, jsonString).then(function (httpResponse) {
                    var data = httpResponse.data;
                    if (popupMode !== "modal") {
                        $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                    } else {
                        contextService.insertIntoContext("grid_refreshdata", { data: data.resultObject, panelid: "#modal" }, true);
                        modalService.show(schemaCacheService.getSchemaFromResult(data), data.resultObject.fields, parameters);
                    }
                    return $q.when(data);
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
            redirectFromServerResponse: redirectFromServerResponse,
            goToApplicationView: goToApplicationView,
            openAsModal:openAsModal,
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
    .factory('redirectService', ['$http', '$rootScope', '$log', '$q', 'contextService', 'fixHeaderService', 'restService', 'applicationService', 'alertService','modalService','schemaCacheService', '$timeout', redirectService]);

})(angular);

