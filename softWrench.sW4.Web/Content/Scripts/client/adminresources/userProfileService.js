﻿
(function (angular) {
    'use strict';



    /*@ngNoInject*/ function userProfileService($q, $rootScope, $log, restService, validationService, contextService, crudContextHolderService, redirectService, alertService, modalService, fixHeaderService) {

        var simpleLog = $log.get("userProfileService", ["profile"]);

        function onSchemaLoad() {
            simpleLog.debug("initing transientprofile data");
            $rootScope["#transientprofiledata"] = {};
        }


        function afterSchemaListLoaded(parameters) {
            simpleLog.debug("schema provider has changed");
            const dm = parameters.fields;
            const options = parameters.options;
            if (!!options && options.length > 0) {
                dm["schema"] = options[0].value;
                //not needed to show schema combo if there´s only one available option. Mode makes more sense to the user
                dm["showschemacombo"] = options.length > 1;
            }
        }

        function afterTabsLoaded(parameters) {
            simpleLog.debug("tabs provider has changed");
            const dm = parameters.fields;
            const options = parameters.options;
            dm["showtabscombo"] = options.length > 1;

            if (options.length === 1) {
                simpleLog.debug("only one tab, selecting it by default");
                dm["#selectedtab"] = options[0].value;
                //not needed to show schema combo if there´s only one available option. Mode makes more sense to the user
            }
        }

        //afterchange
        function tabvaluechanged(parameters) {
            const dm = parameters.fields;
            var tab = parameters.fields["#selectedtab"];
            if (tab == null) {
                //due to mode changes
                return;
            }
            const application = parameters.fields["application"];
            const schemaId = parameters.fields["schema"];
            dm["#fieldPermissions_"] = [];

            simpleLog.debug("tab changed to {0}".format(tab));
            const fullObject = crudContextHolderService.fetchEagerAssociationOptions("selectableTabs").filter(function (item) {
                return item.value === tab;
            })[0];
            if (!fullObject) {
                simpleLog.warn("tab {0} not found from available options".format(tab));
                return;
            }

            //allow creation/allow update flags only make sense for composition(collection)tabs
            const isCompositionTab = fullObject.extrafields["type"] === "ApplicationCompositionDefinition";
            dm["iscompositiontab"] = isCompositionTab;
            const isTab = fullObject.extrafields["type"] === "ApplicationTabDefinition";
            dm["istab"] = isTab;

            if (!isCompositionTab) {
                const queryParameters = {
                    application: application,
                    schemaId: schemaId,
                    tab: tab,
                    pageNumber: 1
                };
                restService.getPromise("UserProfile", "LoadAvailableFields", queryParameters).then(function (httpResponse) {
                    const compositionData = httpResponse.data.resultObject;
                    $rootScope.$broadcast(JavascriptEventConstants.COMPOSITION_RESOLVED, compositionData);
                    mergeTransientIntoDatamap({ tab: tab });
                });
            } else {
                dm["#compallowupdate"] = dm["#compallowcreation"] = true;
                cleanUpCompositions(false);
                mergeTransientIntoDatamap({ tab: tab });
            }

            //resize/position elements
            fixHeaderService.callWindowResize();
        }

        //afterchange
        function afterModeChanged(parameters) {
            const dm = parameters.fields; //cleaning up data
            simpleLog.debug("resting tab");
            dm["schema"] = null;
        }

        //beforechange
        function beforeSchemaChange(parameters) {
            if (!!parameters.oldValue) {
                simpleLog.debug("beforeSchemaChange: storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    schema: parameters.oldValue
                });
            }
        }

        //beforechange
        function beforeTabChange(parameters) {
            if (!!parameters.oldValue) {
                simpleLog.debug("beforeTabChange: storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    tab: parameters.oldValue
                });
            }
        }

        //beforechange
        function beforeApplicationChange(parameters) {
            //before we change the application, let´s store its fresh data into the transientprofiledata object
            if (!!parameters.oldValue && !!parameters.newValue) {
                simpleLog.debug("beforeAppChange:storing updated transient data for {0}".format(parameters.oldValue));
                storeFromDmIntoTransient({
                    "application": parameters.oldValue
                });
            }
        }

        function cleanUpCompositions(cleanUpActions) {
            //cleaning up compositions
            const compositionData = {};
            compositionData["#fieldPermissions_"] = null;
            if (false !== cleanUpActions) {
                compositionData["#actionPermissions_"] = null;
            }

            $rootScope.$broadcast(JavascriptEventConstants.COMPOSITION_RESOLVED, compositionData);
        }

        function resetAssociations() {
            const args = Array.prototype.slice.call(arguments);
            args.forEach(function (association) {
                crudContextHolderService.updateEagerAssociationOptions(association, []);
            });
        }

        //afterchange
        function basicRoleChanged(fieldMetadata, parentdata, fields) {
            var dm = crudContextHolderService.rootDataMap();

            //            const fields = parameters.fields;
            if (fieldMetadata.attribute === "#appallowview") {
                if (fields["#appallowview"] === true) {
                    fields["#appallowcreation"] = false;
                    fields["#appallowupdate"] = false;
                }
            }

            if (fields["#appallowupdate"] || fields["#appallowcreation"]) {
                fields["#appallowview"] = false;
            }

            if (dm["application"] !== fields["#application"]) {
                return;
            }

            dm["anybasicpermission"] = (fields["#appallowview"] || fields["#appallowupdate"] || fields["#appallowcreation"]);

        }

        //afterchange
        function cmpRoleChanged(parameters) {
            const fields = parameters.fields;
            if (parameters.target.attribute === "#compallowview") {
                if (parameters.oldValue === false && parameters.newValue === true) {
                    fields["#compallowcreation"] = false;
                    fields["#compallowupdate"] = false;
                }
            }

            if (fields["#compallowupdate"] || fields["#compallowcreation"]) {
                fields["#compallowview"] = false;
            }

        }

        //afterchange
        function onApplicationChange(parameters) {
            var nextApplication = parameters["#application"];
            if (!parameters["#selected"]) {
                nextApplication = null;
            }

            var dm = crudContextHolderService.rootDataMap();

            //cleaning up data
            dm["schema"] = dm["selectedmode"] = dm["#selectedtab"] = dm["iscompositiontab"] = null;
            dm["application"] = nextApplication;
            dm["anybasicpermission"] = (parameters["#appallowview"] || parameters["#appallowupdate"] || parameters["#appallowcreation"]);

            resetAssociations("selectableTabs", "selectableModes", "schemas");

            cleanUpCompositions();

            var transientData = $rootScope["#transientprofiledata"];
            if (!nextApplication) {
                //scenario where user selects blank entry...
                return $q.when();
            }


            if (!!transientData[nextApplication]) {
                simpleLog.info("application has changed to {0}, but we already have local transient data. no need to fetch from the server".format(nextApplication));
                mergeTransientIntoDatamap({ "application": nextApplication });
                return $q.when();
            }
            const queryParameters = {
                profileId: dm["id"] ? dm["id"] : -1,
                application: nextApplication
            };
            simpleLog.debug("application has changed. No transient data found. fetching application permission from server");


            return restService.getPromise("UserProfile", "LoadApplicationPermissions", queryParameters).then(function (httpResponse) {
                const data = httpResponse.data;
                const appPermission = data.appPermission;
                const hasCreationSchema = data.hasCreationSchema;
                dm["#currentloadedapplication"] = appPermission;

                transientData[nextApplication] = appPermission;
//                transientData[nextApplication]["hasCreationSchema"] = hasCreationSchema;

                mergeTransientIntoDatamap({ "application": nextApplication });

            });
        }

        //afterchange
        function afterSchemaChanged(parameters) {
            const dm = parameters.fields; //cleaning up data
            dm["#selectedtab"] = dm["iscompositiontab"] = null;
            cleanUpCompositions();
            crudContextHolderService.updateEagerAssociationOptions("selectableTabs", []);
            const application = parameters.fields["application"];
            var schemaId = parameters.fields["schema"];
            if (schemaId == null) {
                return;
            }

            simpleLog.debug("schema has been set to {0}, going to server for fetching available actions".format(schemaId));
            const queryParameters = {
                application: application,
                schemaId: schemaId,
                pageNumber: 1
            };
            restService.getPromise("UserProfile", "LoadAvailableActions", queryParameters).then(function (httpResponse) {
                const compositionData = httpResponse.data.resultObject;
                $rootScope.$broadcast(JavascriptEventConstants.COMPOSITION_RESOLVED, compositionData);
                mergeTransientIntoDatamap({ schema: schemaId });
            });

        }

        //method called whenever the list of fields get changed, such as in a pagination event
        function availableFieldsRefreshed(scope, schema, datamap, parameters) {
            if (parameters.relationship === "#fieldPermissions_") {
                simpleLog.debug("list of fields changed for tab {0}".format(parameters.parentdata["#selectedtab"]));
                const compositionData = parameters.previousData;
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
                const compositionData = parameters.previousData;
                if (compositionData && !!parameters.paginationApplied) {
                    storeFromDmIntoTransient({ "#actionPermissions_": compositionData });
                }
            }

        }

        //function activeUsersRefreshed(scope, schema, datamap, parameters) {
        //    if (parameters.relationship === "#users_") {
        //        simpleLog.debug("list of users changed for tab {0}".format(parameters.parentdata["#selectedtab"]));
        //        var compositionData = parameters.previousData;
        //        if (compositionData && !!parameters.paginationApplied) {
        //            storeFromDmIntoTransient({ "#users_": compositionData });
        //        }
        //    }

        //}


        //#region api methods for tests

        function filterAvailablePermissions(item, {compositionItem} = {}) {
            const dm = crudContextHolderService.rootDataMap();
            if (!dm["selectedmode"]) {
                return true;
            }
            if (dm["selectedmode"].equalsAny("grid", "view")) {
                return item.value !== "readonly";
            }

            if (compositionItem && dm["selectedmode"] === "update" && compositionItem["#required"]) {
                return item.value !== "none";
            }
            return true;
        }

        function filterAvailableModes(item) {

            const root = crudContextHolderService.rootDataMap();
            const dm = root["#apppermissions_"].find(a => a["#application"] === root["application"]);
            if (!dm) {
                return false;
            }


            const allowCreation = dm["#appallowcreation"];
            const allowUpdate = dm["#appallowupdate"];
            const allowView = dm["#appallowview"]; //            if (allowCreation && allowUpdate && allowView) {
            //                return true;
            //            }

            if (!allowCreation && item.value === "creation") {
                return false;
            }

            if (!allowUpdate && item.value === "update") {
                return false;
            }

            if (!allowView && item.value === "view") {
                return false;
            }

            return true;

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

            var transientData = $rootScope["#transientprofiledata"];

            var application = dispatcher["application"] ? dispatcher["application"] : dm["application"];
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
            transientAppData["hasCreationSchema"] = dm["hasCreationSchema"];


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

            if (actionPermissionsToIterate) {
                actionPermissionsToIterate.forEach(function (screenAction) {
                    const selected = screenAction["_#selected"];
                    const storedIdx = transientAppData.actionPermissions.findIndex(function (item) {
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
            }

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

                // Store value for hiding containers for all but main container.
                if (dispatcher["tab"] !== 'main') {
                    if (actualContainerPermission["allowView"] != !dm["#hidecontainer"]) {
                        actualContainerPermission["allowView"] = !dm["#hidecontainer"];
                        hasAnyChange = hasAnyChange || true;
                    }
                }

                var fieldPermissionsToIterate = dm["#fieldPermissions_"];

                if (dispatcher["#fieldPermissions_"]) {
                    simpleLog.debug("restoring from fieldPermissions paginate scenario");
                    fieldPermissionsToIterate = dispatcher["#fieldPermissions_"];
                }

                fieldPermissionsToIterate.forEach(function (screnItem) {
                    const idx = actualContainerPermission.fieldPermissions.findIndex(function (storedItem) {
                        return storedItem.fieldKey === screnItem.fieldKey;
                    });
                    if (idx === -1) {
                        if (screnItem.permission !== screnItem.originalpermission) {
                            //no element, let´s add unless we have the fullcontrol mode which is default...
                            simpleLog.debug("adding non default field permission into container for field {0} ".format(screnItem.fieldKey));
                            actualContainerPermission.fieldPermissions.push({
                                permission: screnItem.permission,
                                fieldKey: screnItem.fieldKey
                            });
                            hasAnyChange = hasAnyChange || true;
                        }
                    } else {
                        const storedItem = actualContainerPermission.fieldPermissions[idx];
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
            var application = dm["application"];
            var schema = dm.schema;
            var tab = dm["#selectedtab"];
            const transientAppData = $rootScope["#transientprofiledata"][application];
            if (transientAppData == null) {
                simpleLog.debug("no transient data found for app {0}".format(application));
                return dm;
            }

            simpleLog.info("merge transiet into datamap for app {0}".format(application));

            if (dispatcher["application"]) {

                dm["hasCreationSchema"] = transientAppData.hasCreationSchema;
                //no need to restore this data on every single operation
                dm["#appallowcreation"] = transientAppData.allowCreation && transientAppData.hasCreationSchema;
                dm["#appallowupdate"] = transientAppData.allowUpdate;
                dm["#appallowremoval"] = transientAppData.allowRemoval;
                dm["#appallowview"] = transientAppData.allowView;
                dm["anybasicpermission"] = dm["#appallowview"] || dm["#appallowupdate"] || dm["#appallowcreation"];
                return dm;
            }

            function itemsOfSchema(item) {
                return item.schema === schema;
            }

            //#region schema dispatcher --> action restoring
            if (dispatcher.schema) {
                transientAppData.actionPermissions.filter(itemsOfSchema)
                    .forEach(function (item) {
                        const idx = dm["#actionPermissions_"].findIndex(function (screenItem) {
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
                const container = transientAppData.containerPermissions.firstOrDefault(function (item) {
                    return item.schema === schema && item.containerKey === tab;
                });
                if (container) {
                    container.fieldPermissions.forEach(function (item) {
                        const screenField = dm["#fieldPermissions_"].firstOrDefault(function (screenItem) {
                            return screenItem.fieldKey === item.fieldKey;
                        });
                        if (screenField != null && screenField.permission !== item.permission) {
                            simpleLog.debug("restoring permission of field {0} from {1} to {2}".format(screenField.fieldKey, screenField.permission, item.permission));
                            screenField.permission = item.permission;
                        }
                    });
                    // Restore view setting
                    dm['#hidecontainer'] = container.allowView != undefined ? !container.allowView : false;
                }
            }

            if (dispatcher.tab && transientAppData.compositionPermissions) {
                if (dm["iscompositiontab"] === true && tab) {
                    const cmpData = transientAppData.compositionPermissions.firstOrDefault(function (item) {
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
            const validationArray = validationService.validateCurrent();
            if (validationArray.length > 0) {
                return $q.reject();
            }


            //last storal
            storeFromDmIntoTransient();
            const dm = crudContextHolderService.rootDataMap();

            const appPermissions = dm["#apppermissions_"].map(a => {

                var transientData = $rootScope["#transientprofiledata"][a["#application"]] || {};
                var { containerPermissions, actionPermissions, compositionPermissions } = transientData;


                return {
                    allowCreation: a["#appallowcreation"],
                    allowUpdate: a["#appallowupdate"],
                    allowView: a["#appallowview"],
                    applicationName: a["#application"],
                    containerPermissions,
                    actionPermissions,
                    compositionPermissions,
                    id: a["id"]
                }
            });

            const selectedRoles = dm["#basicroles_"].filter(function (role) {
                return role["_#selected"];
            }).map(function (selectedRole) {
                return { id: selectedRole.id, name: selectedRole.name };
            });
            const ob = {
                applybydefault: dm["applybydefault"],
                id: dm["id"],
                name: dm["name"],
                description: dm["description"],
                applicationPermissions: appPermissions,
                deletable: dm["deletable"] === true || dm["deletable"] === "1" || dm["deletable"] === "true",
                roles: selectedRoles
            };
            return restService.postPromise("UserProfile", "Save", null, ob).then(function (httpResponse) {
                var resultObject = httpResponse.data.resultObject;
                crudContextHolderService.rootDataMap().id = resultObject.id;

                resultObject.applications.forEach(function (resultDTO) {
                    //updating so that id no longer null
                    const app = resultDTO.appPermission;

                    const compEntry = dm["#apppermissions_"].find(a => a["#application"] === app.applicationName);
                    if (compEntry != null) {
                        compEntry["id"] = app.id;
                    }

                    $rootScope["#transientprofiledata"][app.applicationName] = app;
                    $rootScope["#transientprofiledata"][app.applicationName].hasCreationSchema = resultDTO.hasCreationSchema;
                });
            });


        }

        function batchUpdate() {
            const dm = crudContextHolderService.rootDataMap();
            var profileId = dm.id;

            if (!profileId) {
                alertService.alert("Please save the profile before using this action");
                return;
            }


            redirectService.openAsModal("_UserProfile", "batchupdate", {
                title: "Batch Update",
                savefn: function (modaData, modalSchema) {
                    const modaldm = crudContextHolderService.rootDataMap("#modal");
                    const selectedApps = modaldm.applications;
                    if (!selectedApps || selectedApps.length === 0) {
                        alertService.alert("please select at least one application to proceed");
                        return $q.reject();
                    }
                    const params = {
                        profileId: profileId,
                        allowCreation: modaldm.allowcreation,
                        allowUpdate: modaldm.allowupdate,
                        allowView: modaldm.allowview
                    };
                    return restService.postPromise("UserProfile", "BatchUpdate", params, selectedApps).then(function (httpResponse) {
                        $rootScope["#transientprofiledata"] = {};
                        crudContextHolderService.rootDataMap()["application"] = null;
                    });


                }
            });
        }

        function removeMultiple() {
            const dm = crudContextHolderService.rootDataMap();
            const profileId = dm.id;
            if (!profileId) {
                alertService.alert("Please save the profile before using this action");
                return;
            }
            const customParameters = {};
            customParameters[0] = {};
            customParameters[0]["key"] = "profileId";
            customParameters[0]["value"] = profileId;

            return redirectService.openAsModal("person", "userremovelist", {
                title: "Remove Profile from Users",
                searchDTO: {
                    pageSize: 10,
                    customParameters: customParameters
                },
                savefn: function () {
                    const dm = crudContextHolderService.rootDataMap();
                    const profileId = dm.id;
                    const selectedUsers = crudContextHolderService.getSelectionModel('#modal').selectionBuffer;
                    if (!selectedUsers || selectedUsers.length === 0) {
                        alertService.alert("please select at least one user to proceed");
                        return $q.reject();
                    }
                    const usernames = [];
                    for (let user in selectedUsers) {
                        usernames.push(user);
                    }
                    const params = {
                        profileId: profileId
                    };
                    return restService.postPromise("UserProfile", "removeMultiple", params, usernames).then(r => {
                        return modalService.hide();
                    });
                }
            }).catch(err =>
                alertService.alert("There is no user associated to this security group. Operation cannot be executed")
                );
        }

        function applyMultiple() {
            const dm = crudContextHolderService.rootDataMap();
            const profileId = dm.id;
            if (!profileId) {
                alertService.alert("Please save the profile before using this action");
                return;
            }
            const customParameters = {};
            customParameters[0] = {};
            customParameters[0]["key"] = "profileId";
            customParameters[0]["value"] = profileId;
            return redirectService.openAsModal("person", "userselectlist", {
                title: "Apply Profile to Users",
                SearchDTO: {
                    pageSize: 10,
                    customParameters: customParameters
                },
                savefn: function () {
                    const dm = crudContextHolderService.rootDataMap();
                    const profileId = dm.id;
                    const selectedUsers = crudContextHolderService.getSelectionModel('#modal').selectionBuffer;
                    if (!selectedUsers || selectedUsers.length === 0) {
                        alertService.alert("please select at least one user to proceed");
                        return $q.reject();
                    }
                    const usernames = [];
                    for (let user in selectedUsers) {
                        usernames.push(user);
                    }
                    const params = {
                        profileId: profileId
                    };
                    return restService.postPromise("UserProfile", "applyMultiple", params, usernames).then(r => {
                        return modalService.hide();
                    });
                }
            }).catch(err => {
                alertService.alert("All users are already associated to this security group. Operation cannot be executed");
            });
        }
        //#endregion

        function getProfileId() {
            const dm = crudContextHolderService.rootDataMap();
            const profileId = dm.id;
            return [{ "key": "profileId", "value": profileId }];
        }

        function deleteProfile() {
            return alertService.confirm("Are you sure you want to delete this security group? This operation cannot be undone").then(function () {
                const id = crudContextHolderService.rootDataMap()["id"];
                return restService.postPromise("UserProfile", "Delete", { id: id }).then(function (httpResponse) {
                    return redirectService.goToApplicationView("_UserProfile", "list");
                });
            });
        }

        const hooks = {
            afterSchemaListLoaded,
            afterModeChanged,
            afterSchemaChanged,
            afterTabsLoaded,
            availableFieldsRefreshed,
            availableActionsRefreshed,
            //activeUsersRefreshed: activeUsersRefreshed,
            beforeApplicationChange,
            cmpRoleChanged,
            basicRoleChanged,
            beforeTabChange,
            beforeSchemaChange,
            onSchemaLoad,
            onApplicationChange,
            tabvaluechanged
        };
        const api = {
            'delete': deleteProfile,
            filterAvailablePermissions,
            filterAvailableModes,
            mergeTransientIntoDatamap,
            refreshCache,
            storeFromDmIntoTransient,
            save,
            getProfileId
        };
        const actions = {
            batchUpdate: batchUpdate,
            applyMultiple: applyMultiple,
            removeMultiple: removeMultiple
        };
        return angular.extend({}, hooks, api, actions);

    }


    angular.module('sw_crudadmin').service('userProfileService', ['$q', "$rootScope", "$log", 'restService', 'validationService', 'contextService', 'crudContextHolderService', 'redirectService', 'alertService', 'modalService', 'fixHeaderService', userProfileService]);

})(angular);
