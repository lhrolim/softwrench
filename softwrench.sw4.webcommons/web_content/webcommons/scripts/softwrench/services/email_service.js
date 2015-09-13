(function (angular) {
    'use strict';

    

    function emailService($http, $rootScope) {
      
        var emailRegexp = new RegExp("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAddress"></param>
        function validateEmailAddress(emailAddress) {
            if (emailAddress == null) {
                return false;
            }
            return emailRegexp.test(emailAddress.toLowerCase().trim());
        }

        var service = {
            validateEmailAddress: validateEmailAddress
        };

        return service;
    }

    angular.module('webcommons_services').factory('emailService', ['$http', '$rootScope', emailService]);
})(angular);
