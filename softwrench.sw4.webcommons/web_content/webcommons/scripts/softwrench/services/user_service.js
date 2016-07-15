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
            const user = contextService.getUserData();
            if (propertyExpression.startsWith("@user.")) {
                const propName = propertyExpression.substring(6);
                if (user.hasOwnProperty(propName)) {
                    return user[propName];
                }
                return user.genericproperties[propName];
            }
            else if (propertyExpression.equalsAny("@userid")) {
                return user.username;
            }
            else if (propertyExpression.equalsAny("@personid", "@username")) {
                return user.maximoPersonId == null ? user.username : user.maximoPersonId;
            }
            //TODO: finish this;
            return propertyExpression;

        };

        function getPersonId() {
            const user = contextService.getUserData();
            const personId = user.maximoPersonId;
            if (!personId && contextService.isLocal() && "swadmin".equalsIc(user.login)) {
                return "SWADMIN";
            }
            return personId;
        }

        function hasRole(roleArray) {
            if (roleArray == null) {
                return true;
            }
            const user = contextService.getUserData();
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
            if (groupName == null) {
                return true;
            }
            const user = contextService.getUserData();
            
            const personGroups = user.personGroups;
            if (!personGroups || personGroups.length === 0) {
                //fallingback to generic property
                const groupsFromProperty = user.genericproperties["persongroups"] || [];
                return groupsFromProperty.some(s => s === groupName);
            }
            for (let i = 0; i < personGroups.length; i++) {
                const userGroup = personGroups[i];
                if (userGroup.personGroup.name === groupName) {
                    return true;
                }
            }


            return false;
        };

        const service = {
            getPersonId,
            hasRole,
            inGroup,
            readProperty
        };
        return service;

    }
})(angular);