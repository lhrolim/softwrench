﻿(function (angular) {
    'use strict';


    function redirectService($http, $rootScope, $log, $q, contextService, fixHeaderService, restService, applicationService, alertService, modalService, schemaCacheService, $timeout, searchService, historyService, $location) {


        function getActionUrl(controller, action, parameters) {
            return restService.getActionUrl(controller, action, parameters);
        };

        function redirectToHome() {
            contextService.deleteFromContext("swGlobalRedirectURL");
            $location.path("");
            $location.url($location.path());
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
                const w = window.open(redirectUrl);
                w.moveTo(0, 0);
                return $q.when();
            }

            redirectUrl = restService.getActionUrl(controller, action, parameters);

            if (!parameters || !parameters.popupmode) {
                if (controller !== "EntityMetadata" || action !== "Refresh") {
                    //let´s disregard popupmode here, otherwise F5 would refresh popup 
                    contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
                }
            }

            return $http.get(redirectUrl).success(
                function (data) {
                    if (!parameters || !parameters.popupmode) {
                        if (controller !== "EntityMetadata" || action !== "Refresh") {
                            historyService.addToHistory(redirectUrl);
                        }
                    }

                    if (data.type !== "BlankApplicationResponse") {
                        $rootScope.$broadcast("sw_redirectactionsuccess", data);
                    }
                }).error(
                function (data) {
                    const errordata = {
                        errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                        errorStack: data.message
                    };
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
                const tab = $('a[href="#' + tabId + '"]');
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


        function redirectWithData(applicationName, schemaId, searchData, extraParameters) {
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
            if (!schemaId) {
                schemaId = "list";
            }

            var searchOperator = {};
            if (extraParameters.searchOperator) {
                searchOperator = extraParameters.searchOperator;
            }

            var mode = extraParameters.mode ? extraParameters.mode : "none";
            const log = $log.getInstance("redirectService#redirectWithData");
            let searchDTO = extraParameters.searchDTO;

            if (!searchDTO) {
                searchDTO = searchService.buildSearchDTO(searchData, extraParameters.searchSort, searchOperator);
                searchDTO.quickSearchDTO = extraParameters.quickSearchDTO;
                searchDTO.pageNumber = extraParameters.pageNumber ? extraParameters.pageNumber : 1;
                searchDTO.totalCount = 0;
                searchDTO.pageSize = extraParameters.pageSize ? extraParameters.pageSize : 30;
            }

            const restParameters = {
                key: {
                    schemaId: schemaId,
                    mode: mode,
                    platform: "web"
                },
                SearchDTO: searchDTO
            };
            const queryString = $.param(restParameters);
            const urlToUse = url("/api/Data/{0}?{1}".format(applicationName, queryString));
            log.info("invoking url {0}".format(urlToUse));
            var jsonData = {};
            historyService.addToHistory(urlToUse);
            return $http.get(urlToUse).then(function (response) {
                const data = response.data;
                jsonData = data;
                innerGoToApplicationGet(data, null, null, mode, applicationName, null, extraParameters);
                return data;
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
            return this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData)
                .then(resultObject => 
                    contextService.insertIntoContext("grid_refreshdata", { data: resultObject, panelid: "#modal" }, true)
                );
        };

        /**
         * 
         * @param {type} applicationName name of the application to redirect
         * @param {type} schemaId id of the schema to redirect to
         * @param {type} parameters: any parameters to be passed within the query string plus
         *      popupmode -- modal, browser or null for redirecting the main page;
         *      savefn -- a savefn to be executed on popupmode;
         *      onloadfn -- a function to be executed on popupmode;
         * @param {type} jsonData a initial data that could be passed to open a schema filled with some values
         */
        function goToApplication(applicationName, schemaId, parameters, jsonData) {
            parameters = parameters || {};
            return this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData);
        };

        function redirectFromServerResponse(serverResponse, mode) {
            mode = mode || "input";
            return $rootScope.$broadcast("sw_redirectapplicationsuccess", serverResponse, mode, serverResponse.applicationName);
        };

        function goToApplicationView(applicationName, schemaId, mode, title, parameters, jsonData, afterRedirectHook, type) {
            const log = $log.getInstance('redirectService#goToApplication', ["redirect"]);
            parameters = parameters || {};

            $rootScope.$broadcast('sw_applicationredirected', parameters);

            // let´s exclude functions from possible parameters, otherwise it would be evaluated by $.param
            const savefn = parameters.savefn;
            const onloadfn = parameters.onloadfn;
            const postProcessFn = parameters.postProcessFn;
            delete parameters.savefn;
            delete parameters.onloadfn;
            delete parameters.postProcessFn;
            // building url without the function parameters
            var redirectUrl = applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData, type);
            //including back savefn param
            parameters.savefn = savefn;
            parameters.postProcessFn = postProcessFn;
            parameters.onloadfn = onloadfn;

            var popupMode = parameters.popupmode;

            if (popupMode === "browser") {
                window.open(redirectUrl);

                //to keep promise consitent
                return $q.when();
            }

            //this code will get called when the user is already on a crud page and tries to switch view only.
            $rootScope.popupmode = popupMode;
            fixHeaderService.unfix();

            if (jsonData == undefined) {
                if (redirectUrl && !popupMode) {
                    historyService.addToHistory(redirectUrl, parameters.saveHistoryReturn,true);
                }

                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                return $http.get(redirectUrl).then(function (httpResponse) {
                    const data = httpResponse.data;
                    if (angular.isFunction(parameters.postProcessFn)) {
                        parameters.postProcessFn(data);
                    }
                    innerGoToApplicationGet(data, popupMode, redirectUrl, mode, applicationName, afterRedirectHook, parameters);
                    return $q.when(data);
                });
            } else {
                const jsonString = angular.toJson(jsonData);
                if (log.isLevelEnabled("info")) {
                    log.info('invoking post on datacontroller for {0} | content: '.format(applicationName, jsonString));
                }
                return $http.post(redirectUrl, jsonString).then(httpResponse =>{
                    const data = httpResponse.data;
                    if (angular.isFunction(parameters.postProcessFn)) {
                        parameters.postProcessFn(data);
                    }
                    if (popupMode !== "modal") {
                        $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                    } else {
                        contextService.insertIntoContext("grid_refreshdata", { data: data.resultObject, panelid: "#modal" }, true);
                        modalService.show(schemaCacheService.getSchemaFromResult(data), data.resultObject, parameters);
                    }
                    return $q.when(data);
                });
            }
        };

        function innerGoToApplicationGet(data, popupMode, redirectUrl, mode, applicationName, afterRedirectHook, parameters) {
            if (popupMode !== "modal") {
                if (redirectUrl) {
                    contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
                }
                $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
            } else {
                contextService.insertIntoContext("grid_refreshdata", { data: data.resultObject, panelid: "#modal" }, true);
                modalService.show(schemaCacheService.getSchemaFromResult(data), data.resultObject, parameters);
            }
            if (afterRedirectHook != null) {
                afterRedirectHook();
            }
        }

        function redirectNewWindow(newWindowURL, needReload, initialData) {

            if (contextService.isLocal()) {
                //easier to debug on chrome like this
                const w = window.open(newWindowURL);
                w.moveTo(0, 0);
                return;
            }
            const cbk = function (view) {
                const x = window.open('', '_blank', 'height=600px,width=800px,left=350px,top=100px,resizable=yes,scrollbars=yes', false);
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
                const jsonString = angular.toJson(initialData);
                $http.post(newWindowURL, jsonString).success(cbk);
            }

        }

        const service = {
            getActionUrl,
            getApplicationUrl,
            goToAction,
            goToApplication,
            redirectFromServerResponse,
            goToApplicationView,
            openAsModal,
            redirectNewWindow,
            redirectToAction,
            redirectToHome,
            redirectToTab,
            redirectWithData
        };
        return service;
    }

    angular
    .module('sw_layout')
    .factory('redirectService', ['$http', '$rootScope', '$log', '$q', 'contextService', 'fixHeaderService', 'restService', 'applicationService', 'alertService', 'modalService', 'schemaCacheService', '$timeout', 'searchService', 'historyService', "$location", redirectService]);

})(angular);

