var app = angular.module('sw_layout');

app.factory('redirectService', function ($http, $rootScope, $log, contextService, fixHeaderService, restService,applicationService) {

    var buildApplicationURLForBrowser = function (applicationName, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        var currentModule = contextService.retrieveFromContext('currentmodule');
        var currentMetadata = contextService.retrieveFromContext('currentmetadata');

        parameters.currentmodule = currentModule;
        parameters.currentmetadata = currentMetadata;
        var params = $.param(parameters);
        params = replaceAll(params, "=", "$");
        params = replaceAll(params, "&", "@");
        crudUrl = crudUrl + "?application=" + applicationName + "&popupmode=browser";
        if (!nullOrUndef(currentModule)) {
            crudUrl += "&currentModule=" + currentModule;
        }
        if (!nullOrUndef(currentMetadata)) {
            crudUrl += "&currentMetadata=" + currentMetadata;
        }
        crudUrl = crudUrl + "&querystring=" + params;
        return removeEncoding(crudUrl);
    };






    return {


        getActionUrl: function (controller, action, parameters) {
            return restService.getActionUrl(controller, action, parameters);
        },

        redirectToHome: function () {
            contextService.deleteFromContext("swGlobalRedirectURL");
            window.location.reload();
        },

        redirectToAction: function (title, controller, action, parameters, target) {
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
                return;
            }

            redirectUrl = restService.getActionUrl(controller, action, parameters);

            contextService.insertIntoContext("swGlobalRedirectURL", redirectUrl, false);
            $http.get(redirectUrl).success(
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
                });

        },

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters) {
            return applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters);
        },


        redirectToTab: function (tabId) {
            var tab = $('a[href="#' + tabId + '"]');
            tab.trigger('click');;
        },

        goToAction: function (controller, action, parameters) {
            /// <summary>
            /// Shortcut method for the most common case
            /// </summary>
            /// <param name="title"></param>
            /// <param name="controller"></param>
            /// <param name="action"></param>
            /// <param name="parameters"></param>
            /// <param name="target"></param>
            return this.redirectToAction(null, controller, action, parameters, null);
        },

        goToApplication: function (applicationName, schemaId, parameters, jsonData) {
            this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData);
        },

        redirectWithData: function (applicationName, schemaId, mode, searchData, extraParameters) {
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
            $http.get(urlToUse).success(function(data) {
                jsonData = data;
                this.goToApplicationView(applicationName, schemaId, null, null, null,jsonData);
                }).
                error(function(data) {

                });
        },

        

        goToApplicationView: function (applicationName, schemaId, mode, title, parameters, jsonData, afterRedirectHook, type) {
            var log = $log.getInstance('redirectService#goToApplication');

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = applicationService.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData, type);
            var popupMode = parameters.popupmode;
            if (popupMode == "report") {
                //does not popup any window for incident detail report
                //TODO: is this really necessary?
                return;
            }

            if (popupMode == "browser") {
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
                contextService.insertIntoContext("swGlobalRedirectURL", redirectURL, false);

                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                $http.get(redirectURL).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                    if (afterRedirectHook != null) {
                        afterRedirectHook();
                    }
                });
            } else {
                var jsonString = angular.toJson(jsonData);
                log.info('invoking post on datacontroller for {0} | content: '.format(applicationName, jsonString));
                $http.post(redirectURL, jsonString).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                });
            }
        },



        redirectNewWindow: function (newWindowURL, needReload, initialData) {

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
    };

});


