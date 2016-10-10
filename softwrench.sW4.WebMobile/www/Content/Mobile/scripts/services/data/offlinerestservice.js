(function (mobileServices, $) {
    "use strict";

    function offlineRestService($log, $http, settingsService) {
        //#region Utils
        function getActionUrl(controller, action, parameters) {
            action =  action || "get";
            var params = parameters || {};
            return settingsService.getServerUrl().then(function(url) {
                return `${url}/api/generic/${controller}/${action}?${$.param(params, true)}`;
            });
        }
        //#endregion

        //#region Public methods
        function post(controller, action, params, json) {
            return getActionUrl(controller, action, params).then(function(url) {
                $log.get("offlineRestService#post").info("invoking post on url", url);
                return $http.post(url, json);
            });
        }
        function get(controller, action, params) {
            return getActionUrl(controller, action, params).then(function (url) {
                $log.get("offlineRestService#get").info("invoking get on url", url);
                return $http.get(url);
            });
        }
        //#endregion

        //#region Service Instance
        const service = {
            get,
            post
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("offlineRestService", ["$log", "$http", "settingsService", offlineRestService]);
    //#endregion


})(mobileServices, jQuery);
