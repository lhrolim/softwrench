var app = angular.module('sw_layout');

app.factory('userService', function (contextService) {

    "ngInject";

    return {
        //using sessionstorage instead of rootscope, as the later would be lost upon F5.
        //see SWWEB-239


        //determines whether the current user has one of the roles specified on the array
        HasRole: function (roleArray) {
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
        },

        InGroup: function (groupName) {
            if (groupName == null) {
                return true;
            }
            var user = contextService.getUserData();
            var personGroups = user.personGroups;
            for (var i = 0; i < personGroups.length; i++) {
                var userGroup = personGroups[i];
                if (userGroup.personGroup.name == groupName) {
                    return true;
                }
            }
            return false;
        },



    };



});


