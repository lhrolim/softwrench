
(function (angular) {
    'use strict';

    angular
      .module('sw_layout')
      .factory('logoutService', ['$window', 'contextService', 'schemaCacheService', logoutService]);

    function logoutService($window, contextService, schemaCacheService) {


        function logout() {
            if (contextService.isLocal()) {
                //clear local everytime to make development easier
                schemaCacheService.wipeSchemaCacheIfNeeded();
            }
            contextService.clearContext();
            sessionStorage['ctx_loggedin'] = false;
            contextService.deleteFromContext("swGlobalRedirectURL");

        }

        var service = {
            logout: logout
        };

        return service;
    }
})(angular);
