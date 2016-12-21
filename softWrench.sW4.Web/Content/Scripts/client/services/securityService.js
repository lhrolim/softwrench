(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('securityService', function (contextService,alertService) {
    "ngInject";

    return {
        /// <summary>
        /// Some roles have a behaviour that allows the user to see an item on the screen but thrown an error when he tries to perform the action.
        /// This method will do such thing.
        /// </summary>
        /// <param name="role"></param>
        /// <returns type="boolean">true of false so that methods are able to interrupt execution properly</returns>
        validateRoleWithErrorMessage: function (role) {
            if (!role) {
                return true;
            }
            var user = contextService.getUserData();
            var userroles = user.roles;
            var foundRole = null;

            if (!userroles) {
                return true;
            }

            for (var i = 0; i < userroles.length; i++) {
                var userrole = userroles[i];
                if (userrole.name == role) {
                    foundRole= userrole;
                    break;
                }
            }

            if (!foundRole || ( foundRole.authorized != false)) {
                return true;
            }

            alertService.alert(foundRole.unauthorizedMessage);
            return false;
        }

    };

});

})(angular);