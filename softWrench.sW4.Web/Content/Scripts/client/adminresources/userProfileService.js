
(function (angular) {
    'use strict';



    function userProfileService($q, $rootScope, $log, restService, contextService, crudContextHolderService) {

        var simpleLog = $log.get("userProfileService", ["profile"]);

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
            if (tab == null) {
                //due to mode changes
                return;
            }

            var application = parameters.fields["application"];
            var schemaId = parameters.fields["schema"];
            dm["#fieldPermissions_"] = [];

            simpleLog.debug("tab changed to {0}".format(tab));

            var fullObject = crudContextHolderService.fetchEagerAssociationOptions("selectableTabs").filter(function (item) {
                return item.value === tab;
            })[0];
            if (!fullObject) {
                simpleLog.warn("tab {0} not found from available options".format(tab));
                return;
            }

            //allow creation/allow update flags only make sense for composition(collection)tabs
            var isCompositionTab = fullObject.extrafields["type"] === "ApplicationCompositionDefinition";
            dm["iscompositiontab"] = isCompositionTab;
            if (!isCompositionTab) {
                var queryParameters = {
                    application: application,
                    schemaId: schemaId,
                    tab: tab,
                    pageNumber: 1
                }

                restService.getPromise("UserProfile", "LoadAvailableFields", queryParameters).then(function (httpResponse) {
                    var compositionData = httpResponse.data.resultObject;

                    //TODO: merge permissions with local permissions
                    //                    result.forEach(function (value) {
                    //                    });
                    $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
                });
            }



        }

        function afterModeChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            simpleLog.debug("resting tab");
            dm["schema"] = dm["#selectedtab"] = dm["iscompositiontab"] = null;
            cleanUpCompositions();
        }


        function beforeSchemaChange(parameters) {
            if (!!parameters.oldValue) {
                simpleLog.debug("beforeSchemaChange: storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    schema: parameters.oldValue
                });
            }
        }

        function beforeTabChange(parameters) {
            if (!!parameters.oldValue) {
                simpleLog.debug("beforeTabChange: storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    tab: parameters.oldValue
                });
            }
        }

        function beforeApplicationChange(parameters) {
            //before we change the application, let´s store its fresh data into the transientprofiledata object
            if (!!parameters.oldValue) {
                simpleLog.debug("beforeAppChange:storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    application: parameters.oldValue
                });
            }
        }

        function cleanUpCompositions() {
            //cleaning up compositions
            var compositionData = {};
            compositionData["#fieldPermissions_"] = null;
            compositionData["#actionPermissions_"] = null;

            $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
        }

        function resetAssociations() {
            var args = Array.prototype.slice.call(arguments);
            args.forEach(function (association) {
                crudContextHolderService.updateEagerAssociationOptions(association, []);
            });
        }


        function onApplicationChange(parameters) {
            var dm = parameters.fields;
            var nextApplication = dm["application"];

            //cleaning up data
            dm["schema"] = dm["selectedmode"] = dm["#selectedtab"] = dm["iscompositiontab"] = null;

            resetAssociations("selectableTabs", "selectableModes", "schemas");

            cleanUpCompositions();

            var transientData = $rootScope["#transientprofiledata"];
            if (!nextApplication) {
                //scenario where user selects blank entry...
                return $q.when();
            }


            if (!!transientData[nextApplication]) {
                simpleLog.info("application has changed to {0}, but we already have local transient data. no need to fetch from the server".format(nextApplication));
                var dmToRestore = transientData[nextApplication].datamap;


                //we already have local data regarding that application, let´s use it instead of hitting the server.
                //otherwise, we would lose the local state upon app change
                //                parameters.fields = transientData[nextApplication];

                for (var attr in dmToRestore) {
                    if (dmToRestore.hasOwnProperty(attr)) {
                        //we need to keep the same object due to the existing bindings, but change its properties
                        parameters.fields[attr] = dmToRestore[attr];
                    }
                }

                dm["#currentloadedapplication"] = transientData[nextApplication].appPermission;


                //                parameters.scope.datamap = parameters.fields;
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
                    //allowing everything by default
                    dm["#appallowupdate"] = dm["#appallowremoval"] = dm["#appallowviewonly"] = true;
                    return $q.when();
                }

                var permissions = appPermission.collectionPermissions;

                dm["#appallowcreation"] = hasCreationSchema && permissions.allowCreation;
                dm["#appallowupdate"] = permissions.allowUpdate;
                dm["#appallowremoval"] = permissions.allowRemoval;
                dm["#appallowviewonly"] = permission.allowViewOnly;
            });
        }

        function afterSchemaChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            dm["#selectedtab"] = dm["iscompositiontab"] = null;
            crudContextHolderService.updateEagerAssociationOptions("selectableTabs", []);


            var application = parameters.fields["application"];
            var schemaId = parameters.fields["schema"];
            if (schemaId == null) {
                return;
            }

            simpleLog.debug("schema has been set to {0}, going to server for fetching available actions".format(schemaId));

            var queryParameters = {
                application: application,
                schemaId: schemaId,
                pageNumber: 1
            }

            restService.getPromise("UserProfile", "LoadAvailableActions", queryParameters).then(function (httpResponse) {
                var compositionData = httpResponse.data.resultObject;

                //TODO: merge permissions with local permissions
                //                    result.forEach(function (value) {
                //                    });
                $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
            });

        }

        //method called whenever the list of fields get changed, such as in a pagination event
        function availableFieldsRefreshed(scope, schema, datamap, parameters) {
            if (parameters.relationship === "#fieldPermissions_") {
                simpleLog.debug("list of fields changed for tab {0}".format(parameters.parentdata["#selectedtab"]));
                var compositionData = parameters.previousData;
                if (compositionData && !!parameters.paginationApplied) {
                    storeFromDmIntoTransient({ "#fieldPermissions_": compositionData });
                }
            }
        }

        function availableActionsRefreshed(scope, schema, datamap, parameters) {
            if (parameters.relationship === "#actionPermissions_") {
                simpleLog.debug("list of fields changed for tab {0}".format(parameters.parentdata["#selectedtab"]));
                var compositionData = parameters.previousData;
                if (compositionData && !!parameters.paginationApplied) {
                    storeFromDmIntoTransient({ "#actionPermissions_": compositionData });
                }
            }

        }


        //#region api methods for tests

        /**
         * At a given time the screen holds only a portion of the full data that will need to be submitted to the server side.
         * 
         * This action is called whenever there´s a change on the screen that requires the transientdata (the one that will be sent), to be updated.
         * These happens on the beforechange hooks.
         * 
         * For example, the fieldPermissions is paginated, so if we change from page1 to page2 we need to make sure to update the permissions that were changed on page1
         * 
         * @param {} dispatcher the event that initiated the operation, used to filter the operation to a certain level
         * @returns updated transientData 
         */
        function storeFromDmIntoTransient(dispatcher) {

            var dm = crudContextHolderService.rootDataMap();
            if (dm.fields) {
                dm = dm.fields;
            }

            var application = dispatcher.application ? dispatcher.application : dm.application;
            var schema = dispatcher.schema ? dispatcher.schema : dm.schema;
            var tab = dispatcher.tab ? dispatcher.tab : dm["#selectedtab"];
            var isCompositionTab = dm["iscompositiontab"];


            var transientData = $rootScope["#transientprofiledata"];
            var transientAppData = transientData[application];
            if (!transientAppData) {
                //first time we´re changing to an app, no need to merge
                transientAppData = {};
                transientData[application] = transientAppData;
            }
            simpleLog.info("storing datamap into transiet data for app {0}".format(application));

            //basic roles
            transientAppData.allowCreation = dm["#appallowcreation"];
            transientAppData.allowUpdate = dm["#appallowupdate"];
            transientAppData.allowViewOnly = dm["#appallowviewonly"];
            transientAppData.allowRemoval = dm["#appallowremoval"];


            if (!schema) {
                //not reached the schema screen...
                return transientData;
            }

            //#region actionhandling

            transientAppData.actionPermissions = transientAppData.actionPermissions || [];

            var actionPermissionsToIterate = dm["#actionPermissions_"];

            if (dispatcher["#actionPermissions_"]) {
                simpleLog.debug("restoring from actionpermission paginate scenario");
                actionPermissionsToIterate = dispatcher["#actionPermissions_"];
            }


            actionPermissionsToIterate.forEach(function (screenAction) {
                var selected = screenAction["_#selected"];
                var storedIdx = transientAppData.actionPermissions.findIndex(function (item) {
                    return item.actionId === screenAction.actionid;
                });
                if (selected) {
                    if (storedIdx !== -1) {
                        simpleLog.debug("removing action restriction {0} for application {1}".format(screenAction["#actionlabel"], application));
                        //if there was a previous action restriction, remove it
                        transientAppData.actionPermissions.splice(storedIdx, 1);
                    }
                } else if (storedIdx === -1) {
                    simpleLog.debug("adding new action restriction {0} for application {1}".format(screenAction["#actionlabel"], application));
                    transientAppData.actionPermissions.push({
                        schema: schema,
                        actionId: screenAction.actionid
                    });
                }
            });

            //#endregion

            if (!tab) {
                //not reached the tab screen...
                return transientData;
            }

            if (!isCompositionTab) {
                //#region containerhandling --> non compositions
                transientAppData.containerPermissions = transientAppData.containerPermissions || [];
                var containerIndex = transientAppData.containerPermissions.findIndex(function (item) {
                    return item.schema === schema && item.containerKey === tab;
                });

                var actualContainerPermission = {
                    schema: schema,
                    containerKey: tab,
                    fieldPermissions: []
                };

                if (containerIndex !== -1) {
                    actualContainerPermission = transientAppData.containerPermissions[containerIndex];
                }

                var hasAnyChange = false;


                var fieldPermissionsToIterate = dm["#fieldPermissions_"];

                if (dispatcher["#fieldPermissions_"]) {
                    simpleLog.debug("restoring from fieldPermissions paginate scenario");
                    fieldPermissionsToIterate = dispatcher["#fieldPermissions_"];
                }

                fieldPermissionsToIterate.forEach(function (screnItem) {
                    var idx = actualContainerPermission.fieldPermissions.findIndex(function (storedItem) {
                        return storedItem.fieldKey === screnItem.fieldKey;
                    });
                    if (idx === -1) {
                        if (screnItem.permission !== "fullcontrol") {
                            //no element, let´s add unless we have the fullcontrol mode which is default...
                            simpleLog.debug("adding non default field permission into container for field {0} ".format(screnItem.fieldKey));
                            actualContainerPermission.fieldPermissions.push({
                                permission: screnItem.permission,
                                fieldKey: screnItem.fieldKey
                            });
                            hasAnyChange = hasAnyChange || true;
                        }
                    } else {
                        var storedItem = actualContainerPermission.fieldPermissions[idx];
                        hasAnyChange = hasAnyChange || storedItem.permission !== screnItem.permission;
                        if (storedItem.permission !== "fullcontrol" && screnItem.permission === "fullcontrol") {
                            //remove the item since now it is fullcontrol...
                            actualContainerPermission.fieldPermissions.splice(idx, 1);
                        } else if (storedItem.permission !== screnItem.permission) {
                            actualContainerPermission.fieldPermissions[idx].permission = screnItem.permission;
                        }
                    }
                });

                if (hasAnyChange && containerIndex === -1) {
                    transientAppData.containerPermissions.push(actualContainerPermission);
                }
            }
                //#endregion
            else {
                //#region compositionhandling 
                var compAllowCreation = dm["#compallowcreation"];
                var compAllowViewOnly = dm["#compallowviewonly"];
                var compAllowUpdate = dm["#compallowupdate"];
                //                var compAllowRemoval = dm["#compallowremoval"];

                var allDefault = compAllowCreation === compAllowViewOnly === compAllowUpdate === true;
                transientAppData.compositionPermissions = transientAppData.compositionPermissions || [];
                var cmpIndex = transientAppData.compositionPermissions.findIndex(function (item) {
                    return item.compositionKey === tab;
                });
                if (cmpIndex === -1) {
                    if (!allDefault) {
                        simpleLog.debug("adding non default composition permissions entry for application {0} : {1}".format(application, tab));
                        transientAppData.compositionPermissions.push({
                            compositionKey: tab,
                            schema: schema,
                            allowCreation: compAllowCreation,
                            allowViewOnly: compAllowViewOnly,
                            allowUpdate: compAllowUpdate,
                        });
                    }
                } else {
                    var currentCompositionEntry = transientAppData.compositionPermissions[cmpIndex];
                    if (allDefault) {
                        transientAppData.compositionPermissions.splice(cmpIndex, 1);
                    } else {
                        currentCompositionEntry.allowCreation = compAllowCreation;
                        currentCompositionEntry.allowUpdate = compAllowUpdate;
                        currentCompositionEntry.allowViewOnly = compAllowViewOnly
                    }
                }
                //#endregion
            }





            return transientData;
        }

        function restoreFromTransientIntoDatamap(dispatcher) {

        }


        //#endregion

        var service = {
            afterSchemaListLoaded: afterSchemaListLoaded,
            afterModeChanged: afterModeChanged,
            afterSchemaChanged: afterSchemaChanged,
            afterTabsLoaded: afterTabsLoaded,
            availableFieldsRefreshed: availableFieldsRefreshed,
            availableActionsRefreshed: availableActionsRefreshed,
            beforeApplicationChange: beforeApplicationChange,
            beforeTabChange: beforeTabChange,
            beforeSchemaChange: beforeSchemaChange,
            onSchemaLoad: onSchemaLoad,
            onApplicationChange: onApplicationChange,
            restoreFromTransientIntoDatamap: restoreFromTransientIntoDatamap,
            storeFromDmIntoTransient: storeFromDmIntoTransient,
            tabvaluechanged: tabvaluechanged
        };

        return service;
    }


    angular.module('sw_crudadmin').factory('userProfileService', ['$q', "$rootScope", "$log", 'restService', 'contextService', 'crudContextHolderService', userProfileService]);

})(angular);
