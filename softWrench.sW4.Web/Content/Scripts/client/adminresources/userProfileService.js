
(function (angular) {
    'use strict';



    function userProfileService(restService) {



        function onApplicationChange(parameters) {
            var dm = parameters.fields;
            var queryParameters = {
                profileId: dm["id"],
                application: dm["application"]
            };

            restService.getPromise("UserProfile", "LoadApplicationPermissions", queryParameters).then(function (httpResponse) {
                var data = httpResponse.data;
                if (!data || data === "null") {
                    dm["#appallowcreation"] = true;
                    dm["#appallowupdate"] = true;
                    dm["#appallowremoval"] = true;
                    dm["#appallowview"] = true;
                    return;
                }

                var permissions = data.collectionPermissions;
                
                dm["#appallowcreation"] = permissions.allowCreation;
                dm["#appallowupdate"] = permissions.allowUpdate;
                dm["#appallowremoval"] = permissions.allowRemoval;
                dm["#appallowview"] = !permissions.allowCreation && !permissions.allowUpdate && !permissions.allowRemoval;
            });
        }

        var service = {
            onApplicationChange: onApplicationChange
        };

        return service;
    }


    angular.module('sw_crudadmin').factory('userProfileService', ['restService', userProfileService]);

})(angular);
