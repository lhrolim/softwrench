
(function (angular) {
    'use strict';



    function userProfileService($q,$rootScope,$log, restService, contextService, crudContextHolderService) {

        var simpleLog =$log.get("userProfileService", ["profile"]);

        function onSchemaLoad() {
            simpleLog.debug("initing transientprofile data");
            $rootScope["#transientprofiledata"] = {};
        }


        function afterSchemaListLoaded(parameters) {
            simpleLog.debug("schema provider has changed");
            var dm = parameters.fields;
            var options = parameters.options;
            if (!!options && options.length > 0) {
                dm["schema"] = options[0].value;
                //not needed to show schema combo if there´s only one available option. Mode makes more sense to the user
                dm["showschemacombo"] = options.length > 1;
            }
        }

        function afterTabsLoaded(parameters) {
            simpleLog.debug("tabs provider has changed");
            var dm = parameters.fields;
            var options = parameters.options;
            dm["showtabscombo"] = options.length > 1;
            if (options.length === 1) {
                simpleLog.debug("only one tab, selecting it by default");
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


        function beforeApplicationChange(parameters) {
            //before we change the application, let´s store its fresh data into the transientprofiledata object
            var dm = parameters.fields;
            if (!!parameters.oldValue) {
                simpleLog.debug("storing updated transient data for {0}".format(parameters.oldValue));
                var transientData = $rootScope["#transientprofiledata"];
                transientData[parameters.oldValue] = jQuery.extend(true, {}, dm);
                //little fix, since the value has already changed at this point
                transientData[parameters.oldValue]["application"] = parameters.oldValue;
            }
        }

        function onApplicationChange(parameters) {
            var dm = parameters.fields;
            var nextApplication = dm["application"];

            //cleaning up data
            dm["schema"] = dm["selectedmode"] = dm["#selectedtab"] = dm["iscompositiontab"] = null;
            crudContextHolderService.updateEagerAssociationOptions("selectableTabs", []);
            crudContextHolderService.updateEagerAssociationOptions("selectableModes", []);
            crudContextHolderService.updateEagerAssociationOptions("schemas", []);
            var transientData = $rootScope["#transientprofiledata"];
            if (!!transientData[nextApplication]) {
                simpleLog.debug("application has changed, but we already have local transient data. no need to fetch from the server");
                var dmToRestore = transientData[nextApplication];
                //we already have local data regarding that application, let´s use it instead of hitting the server.
                //otherwise, we would lose the local state upon app change
                parameters.fields = transientData[nextApplication];

                for (var attr in dmToRestore) {
                    if (dmToRestore.hasOwnProperty(attr)) {
                        parameters.fields[attr] = dmToRestore[attr];
                    }
                     
                }
                parameters.scope.datamap = parameters.fields;
//                crudContextHolderService.rootDataMap(null, transientData[nextApplication]);
                return $q.when();
            }

            var queryParameters = {
                profileId: dm["id"],
                application: nextApplication
            };

          

            simpleLog.debug("application has changed. No transient data found. fetching application permission from server");


            return restService.getPromise("UserProfile", "LoadApplicationPermissions", queryParameters).then(function (httpResponse) {
                var data = httpResponse.data;
                var appPermission = data.appPermission;
                var hasCreationSchema = data.hasCreationSchema;

                dm["hasCreationSchema"] = hasCreationSchema;
                dm["#currentloadedapplication"] = appPermission;

                if (!appPermission) {
                    dm["#appallowcreation"] = hasCreationSchema;
                    dm["#appallowupdate"] = true;
                    dm["#appallowremoval"] = true;
                    dm["#appallowview"] = true;

                    return $q.when();
                }

                var permissions = appPermission.collectionPermissions;

                dm["#appallowcreation"] = hasCreationSchema && permissions.allowCreation;
                dm["#appallowupdate"] = permissions.allowUpdate;
                dm["#appallowremoval"] = permissions.allowRemoval;
                dm["#appallowview"] = !permissions.allowCreation && !permissions.allowUpdate && !permissions.allowRemoval;
            });
        }

        function afterSchemaChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            dm["#selectedtab"] = dm["iscompositiontab"] = null;
            crudContextHolderService.updateEagerAssociationOptions("selectableTabs", []);
        }

        var service = {
            onSchemaLoad:onSchemaLoad,
            onApplicationChange: onApplicationChange,
            afterSchemaListLoaded: afterSchemaListLoaded,
            afterModeChanged:afterModeChanged,
            afterSchemaChanged:afterSchemaChanged,
            afterTabsLoaded: afterTabsLoaded,
            beforeApplicationChange:beforeApplicationChange,
            tabvaluechanged: tabvaluechanged
        };

        return service;
    }


    angular.module('sw_crudadmin').factory('userProfileService', ['$q',"$rootScope","$log",'restService','contextService', 'crudContextHolderService', userProfileService]);

})(angular);
