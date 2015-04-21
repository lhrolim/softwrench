modules.webcommons.factory('restService', function ($http,$log) {


    return {

        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            var params = parameters == null ? {} : parameters;
            return url("/api/generic/" + controller + "/" + action + "?" + $.param(params));
        },

        invokePost: function (controller, action, queryParameters, json, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokePost");
            log.info("invoking post on url {0}".format(url));
            $http.post(url, json)
                .success(function (data) {
                    if (successCbk != null) {
                        successCbk(data);
                    }
                })
                .error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
        },

        invokeGet: function (controller, action, queryParameters, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            $http.get(url)
                .success(function (data) {
                    if (successCbk != null) {
                        successCbk(data);
                    }
                })
                .error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
        }




    };

});


