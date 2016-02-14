
(function (angular) {
    'use strict';



    function userProfileService(restService, crudContextHolderService) {

        function afterSchemaListLoaded(parameters) {
            var dm = parameters.fields;
            var options = parameters.options;
            if (!!options && options.length > 0) {
                dm["schema"] = options[0].value;
                //not needed to show schema combo if there´s only one available option. Mode makes more sense to the user
                dm["showschemacombo"] = options.length > 1;
            }
        }

        function afterTabsLoaded(parameters) {
            var dm = parameters.fields;
            var options = parameters.options;
            dm["showtabscombo"] = options.length > 1;
            if (options.length === 1) {
                dm["#selectedtab"] = options[0].value;
                //not needed to show schema combo if there´s only one available option. Mode makes more sense to the user
            }
        }

        function tabvaluechanged(parameters) {
            var dm = parameters.fields;
            var tab = parameters.fields["#selectedtab"];
            var fullObject = crudContextHolderService.fetchEagerAssociationOptions("selectableTabs").filter(function (item) {
                return item.value === tab;
            })[0];
            if (fullObject) {
                dm["iscompositiontab"] = fullObject.extrafields["type"] === "ApplicationCompositionDefinition";
            }
        }

        function afterModeChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            dm["#selectedtab"] = dm["iscompositiontab"] = null;
        }

        function onApplicationChange(parameters) {
            var dm = parameters.fields;
            var queryParameters = {
                profileId: dm["id"],
                application: dm["application"]
            };

            //cleaning up data
            dm["schema"] = dm["selectedmode"] = dm["#selectedtab"] = dm["iscompositiontab"] = null;
            crudContextHolderService.updateEagerAssociationOptions("selectableTabs", []);
            crudContextHolderService.updateEagerAssociationOptions("selectableModes", []);
            crudContextHolderService.updateEagerAssociationOptions("schemas", []);


            return restService.getPromise("UserProfile", "LoadApplicationPermissions", queryParameters).then(function (httpResponse) {
                var data = httpResponse.data;
                var appPermission = data.appPermission;
                var hasCreationSchema = data.hasCreationSchema;

                dm["hasCreationSchema"] = hasCreationSchema;


                if (!appPermission) {
                    dm["#appallowcreation"] = hasCreationSchema;
                    dm["#appallowupdate"] = true;
                    dm["#appallowremoval"] = true;
                    dm["#appallowview"] = true;

                    return;
                }

                var permissions = appPermission.collectionPermissions;

                dm["#appallowcreation"] = hasCreationSchema && permissions.allowCreation;
                dm["#appallowupdate"] = permissions.allowUpdate;
                dm["#appallowremoval"] = permissions.allowRemoval;
                dm["#appallowview"] = !permissions.allowCreation && !permissions.allowUpdate && !permissions.allowRemoval;
            });
        }

        var service = {
            onApplicationChange: onApplicationChange,
            afterSchemaListLoaded: afterSchemaListLoaded,
            afterModeChanged:afterModeChanged,
            afterTabsLoaded: afterTabsLoaded,
            tabvaluechanged: tabvaluechanged
        };

        return service;
    }


    angular.module('sw_crudadmin').factory('userProfileService', ['restService', 'crudContextHolderService', userProfileService]);

})(angular);
