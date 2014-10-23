var app = angular.module('sw_layout');

app.factory('redirectService', function ($http, $rootScope, $log, contextService, fixHeaderService, restService) {
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

  

    var getApplicationUrl = function (applicationName, schemaId, mode, title, parameters, jsonData) {
        if (parameters === undefined || parameters == null) {
            parameters = {};
        }
        parameters.key = {
            schemaId: schemaId,
            mode: mode,
            platform: platform()
        };


        if (parameters.popupmode == "browser") {
            return buildApplicationURLForBrowser(applicationName, parameters);
        } else {
            if (title != null && title.trim() != "") {
                parameters.title = title;
            }
            if (jsonData == undefined) {
                return url("/api/data/" + applicationName + "?" + $.param(parameters));
            } else {
                parameters.application = applicationName;
                return url("/api/generic/ExtendedData/OpenDetailWithInitialData" + "?" + $.param(parameters));
            }
        }
    };


    return {

        getActionUrl: function (controller, action, parameters) {
            return restService.getActionUrl(controller, action, parameters);
        },

        redirectToAction: function (title, controller, action, parameters, target) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            if (title != null) {
                parameters.title = title;
            }

            if (target == 'new') {
                var redirectURL = url(controller + "/" + action + "?" + $.param(parameters));
                var w = window.open(redirectURL);
                w.moveTo(0, 0);
            } else {
                var redirectURL = restService.getActionUrl(controller, action, parameters);
                sessionStorage.swGlobalRedirectURL = redirectURL;
                $http.get(redirectURL).success(
                    function (data) {
                        $rootScope.$broadcast("sw_redirectactionsuccess", data);
                    }).error(
                    function (data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                            errorStack: data.message
                        }
                        $rootScope.$broadcast("sw_ajaxerror", errordata);
                    });
            }
        },

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters) {
            return getApplicationUrl(applicationName, schemaId, mode, title, parameters);
        },
       

        redirectToTab: function (tabId) {
            var tab = $('a[href="#' + tabId + '"]');
            tab.trigger('click');;
        },
        
        goToApplication: function(applicationName, schemaId, parameters, jsonData) {
            this.goToApplicationView(applicationName, schemaId, null, null, parameters, jsonData);
        },

        goToApplicationView: function (applicationName, schemaId, mode, title, parameters, jsonData) {
            var log = $log.getInstance('redirectService#goToApplication');

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData);
            var popupMode = parameters.popupmode;
            if (popupMode == "report") {
                //does not popup any window for incident detail report
                //TODO: is this really necessary?
                return;
            }

            if (popupMode == "browser") {
                if ($rootScope.isLocal) {
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
                sessionStorage.swGlobalRedirectURL = redirectURL;

                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                $http.get(redirectURL).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
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

            if ($rootScope.isLocal) {
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
                if ($rootScope.isLocal) {
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


