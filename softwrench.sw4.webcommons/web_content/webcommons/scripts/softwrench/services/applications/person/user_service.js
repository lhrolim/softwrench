(function (angular) {
    'use strict';

    class userService {
        constructor (contextService) {
            this.contextService = contextService;
        }

        locatePrimaryEmail(compositionEmails) {

            if (!compositionEmails) {
                return null;
            }
            for (let i = 0; i < compositionEmails.length; i++) {
                const email = compositionEmails[i];
                if (email.isprimary) {
                    return email.emailaddress;
                }
            }
            return null;
        }

        readProperty(propertyExpression) {
            if (propertyExpression == null) {
                return null;
            }
            if (!propertyExpression.startsWith("@")) {
                return propertyExpression;
            }
            const user = this.contextService.getUserData();
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

        }

        getPersonId() {
            const user = this.contextService.getUserData();
            const personId = user.maximoPersonId;
            if (!personId && this.contextService.isLocal() && "swadmin".equalsIc(user.login)) {
                return "SWADMIN";
            }
            return personId;
        }

        hasRole(roleArray) {
            if (roleArray == null) {
                return true;
            }
            const user = this.contextService.getUserData();
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

        inGroup(groupName, useMaximoGroups=false) {
            if (groupName == null) {
                return true;
            }
            const user = this.contextService.getUserData();
            
            if (useMaximoGroups) {
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
            } 
            return user.profiles && user.profiles.some(p => groupName.equalIc(p.name));
        }

        }

        userService.$inject = ["contextService"];

        angular.module('webcommons_services').service('userService', userService);
        })(angular);