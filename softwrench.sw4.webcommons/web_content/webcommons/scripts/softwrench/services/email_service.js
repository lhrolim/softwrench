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

       
        function hasPrimary() {
            const rootDatamap = crudContextHolderService.rootDataMap();
            var result = false;
            const emailAddresses = rootDatamap["email_"];
            for (let email in emailAddresses) {
                if (!emailAddresses.hasOwnProperty(email)) {
                    continue;
                }
                if (emailAddresses[email]["isprimary"] == 1) {
                    result = true;
                }
            }
            return result;
        }

     

        const service = {
            validateEmailAddress,
            hasPrimary
        };

        return service;
    }

    angular.module('webcommons_services').factory('emailService', ['$http', '$rootScope', 'crudContextHolderService', emailService]);
})(angular);
