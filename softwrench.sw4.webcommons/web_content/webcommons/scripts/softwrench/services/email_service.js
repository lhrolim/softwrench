(function () {
    'use strict';

    angular.module('webcommons_services').factory('emailService', ['$http', '$rootScope', emailService]);

    function emailService($http, $rootScope) {
        var service = {
            validateEmailAddress: validateEmailAddress
        };

        return service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAddress"></param>
        function validateEmailAddress(emailAddress) {
            var emailRegexp = new RegExp("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            return emailRegexp.test(emailAddress);
        }
    }
})();
