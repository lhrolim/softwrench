(function(angular) {
    "use strict";

angular.module('sw_layout')
    .factory('pwdenforceService', ["$rootScope", "$timeout", "i18NService", "alertService", function ($rootScope, $timeout, i18NService, alertService) {

    return {
        checker: function(password, retype) {
            // Enforce password restriction
            if (password != null || retype != null) {
                if (password !== retype) {
                    alertService.alert("Your password does not match, please try again.");
                    return false;
                }

                if (password.length < 6) {
                    alertService.alert("Your password is too short. It must be at least 6 characters long.");
                    return false;
                }
            }

            return true;
        }
    };
}]);

})(angular);