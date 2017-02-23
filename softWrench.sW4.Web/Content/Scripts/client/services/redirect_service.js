var app = angular.module('sw_layout');

app.factory('redirectService', function ($http, $rootScope, $log, contextService, fixHeaderService, restService) {

    "ngInject";

    var adjustCurrentModuleForNewWindow = function (currentModule) {
        var currentModuleNewWindow = contextService.retrieveFromContext('currentmodulenewwindow');
        if (currentModuleNewWindow != "null" && currentModuleNewWindow != "") {
            //this currentmodulenewwindow is used to avoid that the module of the menu changes if a leaf on another module context that opens a browser gets clicked
            //HAP-813
            currentModule = currentModuleNewWindow;
            if (currentModule == null) {
                currentModule = "null";
            }
            contextService.deleteFromContext('currentmodulenewwindow');
        }
        return currentModule;
    };

    var buildApplicationURLForBrowser = function (applicationName, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        var currentModule = contextService.retrieveFromContext('currentmodule');
        currentModule = adjustCurrentModuleForNewWindow(currentModule);
        var currentMetadata = parameters.currentmetadata = "null";
        parameters.currentmodule = currentModule;
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

    var buildActionURLForBrowser = function (controller, action, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        var currentModule = contextService.retrieveFromContext('currentmodule');
        var currentMetadata = parameters.currentmetadata = "null";
        currentModule = adjustCurrentModuleForNewWindow(currentModule);
        parameters.currentmodule = currentModule;
        parameters.currentmetadata = currentMetadata;
        var params = $.param(parameters);
        params = replaceAll(params, "=", "$");
        params = replaceAll(params, "&", "@");
        crudUrl = crudUrl + "?controllerToRedirect=" + controller + "&actionToRedirect=" + action + "&popupmode=browser";
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
            if (parameters.popupmode == "browser") {
                return buildActionURLForBrowser(controller, action, parameters);
            } else {
                action = (action === undefined || action == null) ? 'get' : action;
                var params = parameters == null ? {} : parameters;
                return url("/api/generic/" + controller + "/" + action + "?" + $.param(params));
            }
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
                var redirectURL = this.getActionUrl(controller, action, parameters);

                if (parameters.popupmode == "browser") {
                    if ($rootScope.isLocal && "true" != sessionStorage.defaultpopupsize) {
                        //easier to debug on chrome like this
                        var w = window.open(redirectURL);
                        //                    w.moveto(0, 0);
                    } else {
                        openPopup(redirectURL);
                    }
                    return;
                }

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

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters, jsonData) {
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
                    if (title != null && title.trim() != "") {
                        parameters.title = title;
                    }
                    return url("/api/generic/ExtendedData/OpenDetailWithInitialData" + "?" + $.param(parameters));
                }
            }
        },

        goToApplicationView: function (applicationName, schemaId, mode, title, parameters, jsonData) {
            var log = $log.getInstance('redirectService#goToApplication');

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = this.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData);
            var popupMode = parameters.popupmode;
            if (popupMode == "report") {
                //does not popup any window for incident detail report
                //TODO: is this really necessary?
                return;
            }

            if (popupMode == "browser") {
                restService.getPromise("ExtendedData", "PingServer").then(function () {
                    if ($rootScope.isLocal && "true" != sessionStorage.defaultpopupsize) {
                        //easier to debug on chrome like this
                        var w = window.open(redirectURL);
                        //                    w.moveto(0, 0);
                    } else {
                        openPopup(redirectURL);
                    }
                });
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

        redirectToTab: function (tabId) {
            var tab = $('a[href="#' + tabId + '"]');
            tab.trigger('click');
        },


        redirectNewWindow: function (newWindowURL, needReload, initialData) {
            restService.getPromise("ExtendedData", "PingServer").then(function () {
                if ($rootScope.isLocal) {
                    //easier to debug on chrome like this
                    var w = window.open(newWindowURL);
                    w.moveTo(0, 0);
                    return;
                }

                var cbk = function (view) {
                    var x = openPopup('');

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
            });
        }
    };

});

function openPopup(redirectURL) {
    var width = 1024;
    var height = 768;

    var x = screen.width / 2 - width / 2;
    var y = screen.height / 2 - height / 2;

    var w = window.open(redirectURL, '_blank', 'height=' + height + 'px,width=' + width + 'px,left=' + x + ',top=' + y + ',resizable=yes,scrollbars=yes', false);
    w.focus();

    return w;
}
