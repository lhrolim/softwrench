
(function (angular) {
    'use strict';



    function userProfileService($q, $rootScope, $log, restService, contextService, crudContextHolderService, redirectService, alertService, modalService) {

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
                    $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
                    mergeTransientIntoDatamap({ tab: tab });
                });
            } else {
                dm["#compallowupdate"] = dm["#compallowcreation"] = dm["#compallowview"] = true;
                mergeTransientIntoDatamap({ tab: tab });
            }
        }

        function afterModeChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            simpleLog.debug("resting tab");
            dm["schema"] = null;
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
            if (!!parameters.oldValue && !!parameters.newValue) {
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

        function allowUpdateChanged(parameters) {
            if (!!parameters.fields["#appallowupdate"]) {
                parameters.fields["#appallowview"] = true;
            }
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
                mergeTransientIntoDatamap({ application: nextApplication });
                return $q.when();
            }


            var queryParameters = {
                profileId: dm["id"] ? dm["id"] : -1,
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
                    dm["#appallowupdate"] = dm["#appallowremoval"] = dm["#appallowview"] = true;
                    return $q.when();
                }

                transientData[nextApplication] = appPermission;
                transientData[nextApplication]["hasCreationSchema"] = hasCreationSchema;

                mergeTransientIntoDatamap({ application: nextApplication });

            });
        }

        function afterSchemaChanged(parameters) {
            var dm = parameters.fields;
            //cleaning up data
            dm["#selectedtab"] = dm["iscompositiontab"] = null;
            cleanUpCompositions();
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
                $rootScope.$broadcast("sw_compositiondataresolved", compositionData);
                mergeTransientIntoDatamap({ schema: schemaId });
            });

        }

        //method called whenever the list of fields get changed, such as in a pagination event
        function availableFieldsRefreshed(scope, schema, datamap, parameters) {
            if (parameters.relationship === "#fieldPermissions_") {
                simpleLog.debug("list of fields changed for tab {0}".format(parameters.parentdata["#selectedtab"]));
                var compositionData = parameters.previousData;
                if (compositionData && !!parameters.paginationApplied) {
                    //first store old page
                    storeFromDmIntoTransient({ "#fieldPermissions_": compositionData });
                    //now merge new page
                    mergeTransientIntoDatamap({ "#fieldPermissions_": parameters.clonedCompositionData })
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

        function filterAvailablePermissions(item) {
            var dm = crudContextHolderService.rootDataMap().fields;
            if (!dm["selectedmode"] || dm["selectedmode"] !== "grid") {
                return true;
            }
            return item.value !== "readonly";
        }

        function refreshCache() {
            restService.postPromise("UserProfile", "RefreshCache");
        }


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
            dispatcher = dispatcher || {};

            var dm = crudContextHolderService.rootDataMap();
            if (dm.fields) {
                dm = dm.fields;
            }

            var transientData = $rootScope["#transientprofiledata"];

            var application = dispatcher.application ? dispatcher.application : dm.application;
            if (!application) {
                //save method called on blank application
                return transientData;
            }

            var schema = dispatcher.schema ? dispatcher.schema : dm.schema;
            var tab = dispatcher.tab ? dispatcher.tab : dm["#selectedtab"];
            var isCompositionTab = dm["iscompositiontab"];



            var transientAppData = transientData[application];
            if (!transientAppData) {
                //first time we´re changing to an app, no need to merge
                transientAppData = {
                    applicationName: application
                };
                transientData[application] = transientAppData;
            }
            simpleLog.info("storing datamap into transiet data for app {0}".format(application));

            function storeIfDiffers(transientPropName, propName, transientObj) {
                transientObj = transientObj || transientAppData;

                if (transientObj[transientPropName] !== dm[propName]) {
                    transientObj[transientPropName] = dm[propName];
                    transientAppData["_#isDirty"] = true;
                }

            }


            //basic roles

            storeIfDiffers("allowCreation", "#appallowcreation");
            storeIfDiffers("allowUpdate", "#appallowupdate");
            storeIfDiffers("allowView", "#appallowview");
            storeIfDiffers("allowRemoval", "#appallowremoval");



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
                        transientAppData["_#isDirty"] = true;
                    }
                } else if (storedIdx === -1) {
                    simpleLog.debug("adding new action restriction {0} for application {1}".format(screenAction["#actionlabel"], application));
                    transientAppData.actionPermissions.push({
                        schema: schema,
                        actionId: screenAction.actionid
                    });
                    transientAppData["_#isDirty"] = true;
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

                if (hasAnyChange || containerIndex === -1) {
                    transientAppData["_#isDirty"] = true;
                    transientAppData.containerPermissions.push(actualContainerPermission);
                }
            }
                //#endregion
            else {
                //#region compositionhandling 
                var compAllowCreation = dm["#compallowcreation"];
                var compAllowView = dm["#compallowview"];
                var compAllowUpdate = dm["#compallowupdate"];
                //                var compAllowRemoval = dm["#compallowremoval"];

                var allDefault = compAllowCreation === true && compAllowView === true && compAllowUpdate === true;
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
                            allowView: compAllowView,
                            allowUpdate: compAllowUpdate,
                        });
                        transientAppData["_#isDirty"] = true;
                    }
                } else {
                    var currentCompositionEntry = transientAppData.compositionPermissions[cmpIndex];
                    if (allDefault) {
                        transientAppData["_#isDirty"] = true;
                        transientAppData.compositionPermissions.splice(cmpIndex, 1);
                    } else {
                        storeIfDiffers("allowCreation", "#compallowcreation", currentCompositionEntry);
                        storeIfDiffers("allowUpdate", "#compallowupdate", currentCompositionEntry);
                        storeIfDiffers("allowView", "#compallowview", currentCompositionEntry);
                        storeIfDiffers("allowRemoval", "#compallowremoval", currentCompositionEntry);
                    }
                }
                //#endregion
            }
            return transientData;
        }

        function mergeTransientIntoDatamap(dispatcher) {
            var dm = crudContextHolderService.rootDataMap();
            if (dm.fields) {
                dm = dm.fields;
            }
            var application = dm.application;
            var schema = dm.schema;
            var tab = dm["#selectedtab"];

            var transientAppData = $rootScope["#transientprofiledata"][application];
            if (transientAppData == null) {
                simpleLog.debug("no transient data found for app {0}".format(application));
                return dm;
            }

            simpleLog.info("merge transiet into datamap for app {0}".format(application));

            if (dispatcher.application) {

                dm["hasCreationSchema"] = transientAppData.hasCreationSchema;
                //no need to restore this data on every single operation
                dm["#appallowcreation"] = transientAppData.allowCreation && transientAppData.hasCreationSchema;
                dm["#appallowupdate"] = transientAppData.allowUpdate;
                dm["#appallowremoval"] = transientAppData.allowRemoval;
                dm["#appallowview"] = transientAppData.allowView;
                return dm;
            }

            function itemsOfSchema(item) {
                return item.schema === schema;
            }

            //#region schema dispatcher --> action restoring
            if (dispatcher.schema) {
                transientAppData.actionPermissions.filter(itemsOfSchema)
                    .forEach(function (item) {
                        var idx = dm["#actionPermissions_"].findIndex(function (screenItem) {
                            return screenItem.actionid === item.actionId;
                        });
                        if (idx !== -1) {
                            simpleLog.debug("restoring permission of action {0} to false".format(item.actionId));
                            dm["#actionPermissions_"][idx]["_#selected"] = false;
                        }
                    });
            }

            //#endregion

            //#region field permission restore (tab or paginated dispatcher)
            if ((dispatcher.tab || dispatcher["#fieldPermissions_"]) && transientAppData.containerPermissions) {
                var container = transientAppData.containerPermissions.firstOrDefault(function (item) {
                    return item.schema === schema && item.containerKey === tab;
                });
                if (container) {
                    container.fieldPermissions.forEach(function (item) {

                        var screenField = dm["#fieldPermissions_"].firstOrDefault(function (screenItem) {
                            return screenItem.fieldKey === item.fieldKey;
                        });
                        if (screenField != null && screenField.permission !== item.permission) {
                            simpleLog.debug("restoring permission of field {0} from {1} to {2}".format(screenField.fieldKey, screenField.permission, item.permission));
                            screenField.permission = item.permission;
                        }
                    });
                }
            }

            if (dispatcher.tab && transientAppData.compositionPermissions) {
                if (dm["iscompositiontab"] === true && tab) {
                    var cmpData = transientAppData.compositionPermissions.firstOrDefault(function (item) {
                        return item.schema === schema && item.compositionKey === tab;
                    });
                    if (cmpData) {
                        dm["#compallowcreation"] = cmpData.allowCreation;
                        dm["#compallowupdate"] = cmpData.allowUpdate;
                        dm["#compallowremoval"] = cmpData.allowRemoval;
                        dm["#compallowview"] = cmpData.allowView;
                    }
                }
            }


            //#endregion



            return dm;
        }

        function save() {
            //last storal
            storeFromDmIntoTransient();
            var dm = crudContextHolderService.rootDataMap().fields;
            var appPermissions = Object.keys($rootScope["#transientprofiledata"])
                .map(function (key) {
                    return $rootScope["#transientprofiledata"][key];
                }).filter(function (ob) {
                    return ob["_#isDirty"];
                });

            //TODO: add pagination support here
            var selectedRoles = dm["#basicroles_"].filter(function (role) {
                return role["_#selected"];
            }).map(function (selectedRole) {
                return { id: selectedRole.id, name: selectedRole.name };
            });


            var ob = {
                id: dm["id"],
                name: dm["name"],
                description: dm["description"],
                applicationPermissions: appPermissions,
                roles: selectedRoles
            }

            return restService.postPromise("UserProfile", "Save", null, ob).then(function (httpResponse) {
                var resultObject = httpResponse.data.resultObject;
                crudContextHolderService.rootDataMap().fields.id = resultObject.id;

                resultObject.applications.forEach(function (resultDTO) {
                    //updating so that id no longer null
                    var app = resultDTO.appPermission;
                    $rootScope["#transientprofiledata"][app.applicationName] = app;
                    $rootScope["#transientprofiledata"][app.applicationName].hasCreationSchema = resultDTO.hasCreationSchema;
                });
            });


        }

        function batchUpdate() {
            var dm = crudContextHolderService.rootDataMap();

            var profileId = dm.fields.id;

            if (!profileId) {
                alertService.alert("Please save the profile before using this action");
                return;
            }


            redirectService.openAsModal("_UserProfile", "batchupdate", {
                title: "Batch Update",
                savefn: function (modaData, modalSchema) {
                    var modaldm = crudContextHolderService.rootDataMap("#modal");
                    var selectedApps = modaldm.applications;
                    if (!selectedApps || selectedApps.length === 0) {
                        alertService.alert("please select at least one application to proceed");
                        return $q.reject();
                    }

                    var params = {
                        profileId: profileId,
                        allowCreation: modaldm.allowcreation,
                        allowUpdate: modaldm.allowupdate,
                        allowView: modaldm.allowview
                    }

                    return restService.postPromise("UserProfile", "BatchUpdate", params, selectedApps).then(function (httpResponse) {
                        $rootScope["#transientprofiledata"] = {};
                        crudContextHolderService.rootDataMap()["fields"]["application"] = null;
                    });


                }
            });
        }

        function applyMultiple() {
            var dm = crudContextHolderService.rootDataMap();
            var profileId = dm.fields.id;
            if (!profileId) {
                alertService.alert("Please save the profile before using this action");
                return;
            }
            return redirectService.openAsModal("person", "userselectlist", {
                title: "Apply Profile to Users",
                savefn: function () {
                    var dm = crudContextHolderService.rootDataMap();
                    var profileId = dm.fields.id;
                    var selectedUsers = crudContextHolderService.getSelectionModel('#modal').selectionBuffer;
                    if (!selectedUsers || selectedUsers.length === 0) {
                        alertService.alert("please select at least one user to proceed");
                        return $q.reject();
                    }
                    var usernames = [];
                    for (var user in selectedUsers) {
                        usernames.push(user);
                    }
                    var params = {
                        profileId: profileId
                    }
                    return restService.postPromise("UserProfile", "applyMultiple", params, usernames);
                }
            });
        }
        //#endregion

        function deleteProfile() {
            return alertService.confirm2("Are you sure you want to delete this security group? This operation cannot be undone").then(function () {
                var id = crudContextHolderService.rootDataMap().fields["id"];
                return restService.postPromise("UserProfile", "Delete", { id: id }).then(function (httpResponse) {
                    return redirectService.goToApplicationView("_UserProfile", "list");
                });
            });
        }

        var hooks = {
            afterSchemaListLoaded: afterSchemaListLoaded,
            afterModeChanged: afterModeChanged,
            afterSchemaChanged: afterSchemaChanged,
            afterTabsLoaded: afterTabsLoaded,
            allowUpdateChanged: allowUpdateChanged,
            availableFieldsRefreshed: availableFieldsRefreshed,
            availableActionsRefreshed: availableActionsRefreshed,
            beforeApplicationChange: beforeApplicationChange,
            beforeTabChange: beforeTabChange,
            beforeSchemaChange: beforeSchemaChange,
            onSchemaLoad: onSchemaLoad,
            onApplicationChange: onApplicationChange,
            tabvaluechanged: tabvaluechanged
        };

        var api = {
            'delete': deleteProfile,
            filterAvailablePermissions: filterAvailablePermissions,
            mergeTransientIntoDatamap: mergeTransientIntoDatamap,
            refreshCache: refreshCache,
            storeFromDmIntoTransient: storeFromDmIntoTransient,
            save: save
        }

        var actions = {
            batchUpdate: batchUpdate,
            applyMultiple: applyMultiple
        }

        return angular.extend({}, hooks, api, actions);

    }


    angular.module('sw_crudadmin').factory('userProfileService', ['$q', "$rootScope", "$log", 'restService', 'contextService', 'crudContextHolderService', 'redirectService', 'alertService', 'modalService', userProfileService]);

})(angular);
