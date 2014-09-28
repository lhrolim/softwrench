var app = angular.module('sw_layout');

app.factory('redirectService', function ($http, $rootScope) {
    var buildApplicationURL = function (applicationName, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        if (!nullOrUndef($rootScope.currentmodule)) {
            parameters.currentmodule = $rootScope.currentmodule;
        }
        var params = $.param(parameters);
        params = replaceAll(params, "=", "$");
        params = replaceAll(params, "&", "@");
        crudUrl = crudUrl + "?application=" + applicationName + "&popupmode=browser";
        if (!nullOrUndef($rootScope.currentmodule)) {
            crudUrl += "&currentModule=" + $rootScope.currentmodule;
        }
        crudUrl = crudUrl + "&querystring=" + params;
        return removeEncoding(crudUrl);
    };

    var getActionUrl = function (controller, action, parameters) {
        action = (action === undefined || action == null) ? 'get' : action;
        var params = parameters == null ? {} : parameters;
        return url("/api/generic/" + controller + "/" + action + "?" + $.param(params));
    };

    var getApplicationUrl = function (applicationName, schemaId, mode, title, parameters) {
        if (parameters === undefined || parameters == null) {
            parameters = {};
        }
        parameters.key = {};
        parameters.key.schemaId = schemaId;
        parameters.key.mode = mode;
        parameters.key.platform = platform();

        if (parameters.popupmode == "browser") {
            return buildApplicationURL(applicationName, parameters);
        } else {
            if (title != null && title.trim() != "") parameters.title = title;

            return url("/api/data/" + applicationName + "?" + $.param(parameters));
        }
    };


    return {

        getActionUrl: function (controller, action, parameters) {
            return getActionUrl(controller, action, parameters);
        },

        redirectToAction: function (title, controller, action, parameters) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            if (title != null) {
                parameters.title = title;
            }

            var redirectURL = getActionUrl(controller, action, parameters);
            sessionStorage.swGlobalRedirectURL = redirectURL;
            $http.get(redirectURL).success(
                function (data) {
                    $rootScope.$broadcast("sw_redirectactionsuccess", data);
                });
        },

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters) {
            return getApplicationUrl(applicationName, schemaId, mode, title, parameters);
        },

        goToApplicationView: function (applicationName, schemaId, mode, title, parameters) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = getApplicationUrl(applicationName, schemaId, mode, title, parameters);

            if (parameters.popupmode == "browser") {

                //var w = window.open(redirectURL);
                //w.moveto(0, 0);
                var w = window.open(redirectURL, '_blank', 'height=600px,width=800px,left=350px,top=100px,resizable=yes,scrollbars=yes', false);
                w.focus();
            }
          else {
                //this code will get called when the user is already on a crud page and tries to switch view only.
                sessionStorage.swGlobalRedirectURL = redirectURL;
                $http.get(redirectURL).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                });
            }
        }
    };

});


