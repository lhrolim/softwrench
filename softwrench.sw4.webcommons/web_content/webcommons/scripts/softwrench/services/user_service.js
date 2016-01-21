(function (angular) {
    'use strict';

    angular.module('webcommons_services').factory('userService', ['contextService', userService]);

    function userService(contextService) {



        function readProperty(propertyExpression) {
            if (propertyExpression == null) {
                return null;
            }
            if (!propertyExpression.startsWith("@")) {
                return propertyExpression;
            }

            var user = contextService.getUserData();

            if (propertyExpression.startsWith("@user.")) {
                var propName = propertyExpression.substring(6);
                if (user.hasOwnProperty(propName)) {
                    return user[propName];
                }
                return user.genericproperties[propName];
            }
            else if (propertyExpression.equalsAny("@userid")) {
                return user.username;
            }
            else if (propertyExpression.equalsAny("@personid","@username")) {
                return user.maximoPersonId == null ? user.username : user.maximoPersonId;
            }
            //TODO: finish this;
            return propertyExpression;

        };

        function getPersonId() {
            var user = contextService.getUserData();
            return user.maximoPersonId;
        }

        function hasRole(roleArray) {
            if (roleArray == null) {
                return true;
            }
            var user = contextService.getUserData();
            var userroles = user.roles;
            var result = false;
            $.each(roleArray, function (key, value) {
                $.each(userroles, function (k, v) {
                    if (v.name == value) {
                        result = true;
                        return;
                    }
                });
            });
            return result;
        }

        function inGroup(groupName) {
            if (group == null) {
                return true;
            }
            var user = contextService.getUserData();
            var personGroups = user.personGroups;
            var result = false;

            for (var i = 0; i < personGroups.length; i++) {
                var userGroup = personGroups[i];
                if (userGroup.personGroup.name == groupName) {
                    return true;
                }
            }
            return false;
        };

        var service = {
            getPersonId: getPersonId,
            HasRole: hasRole,
            InGroup: inGroup,
            readProperty: readProperty
        };

        return service;

    }
})(angular);