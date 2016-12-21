
(function (angular) {
    'use strict';

    angular
      .module('sw_layout')
      .service('logoutService', ['$window', 'contextService', 'schemaCacheService', logoutService]);

    function logoutService($window, contextService, schemaCacheService) {


        function logout() {
            if (contextService.isLocal()) {
                //clear local everytime to make development easier
                schemaCacheService.wipeSchemaCacheIfNeeded();
            }
            contextService.clearContext();
            sessionStorage['ctx_loggedin'] = false;
            contextService.deleteFromContext("swGlobalRedirectURL");
            $window.location.href = url('/SignOut/SignOut');
        }

        var service = {
            logout: logout
        };

        return service;
    }
})(angular);
