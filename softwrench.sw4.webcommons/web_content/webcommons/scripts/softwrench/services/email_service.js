(function (angular) {
    'use strict';

    

    function emailService($http, $rootScope, crudContextHolderService) {
      
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

        function isPrimary() {
            return !hasPrimary();
        }

        function hasPrimary() {
            var rootDatamap = crudContextHolderService.rootDataMap();
            var hasPrimary = false;
            var emailAddresses = rootDatamap.fields["email_"];
            for (var email in emailAddresses) {
                if (!emailAddresses.hasOwnProperty(email)) {
                    continue;
                }
                if (emailAddresses[email]["isprimary"] == 1) {
                    hasPrimary = true;
                }
            }
            return hasPrimary;
        }

        var service = {
            validateEmailAddress: validateEmailAddress,
            isPrimary: isPrimary,
            hasPrimary: hasPrimary
        };

        return service;
    }

    angular.module('webcommons_services').factory('emailService', ['$http', '$rootScope', 'crudContextHolderService', emailService]);
})(angular);
