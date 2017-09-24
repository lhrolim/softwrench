(function (angular, _) {
    "use strict";

    function laborService(dao, securityService, localStorageService, crudContextService, $ionicPopup, $q, $log, offlineSchemaService, offlineSaveService, $rootScope, menuModelService) {
        //#region Utils

        const truncateDecimal = value => parseFloat(value.toFixed(2));

        let parentIdCache = null;
        let laborCache = null;

        // init cache
        dao.findUnique("ActiveLaborTracker").then((tracker) => {
            if (!tracker) {
                return;
            }
            
            parentIdCache = tracker.parentid;
            dao.findById("DataEntry", parentIdCache).then((parent) => {
                if (!parent || !parent.datamap || !parent.datamap["labtrans_"]) {
                    return;
                }
                const labTrans = parent.datamap["labtrans_"];
                angular.forEach(labTrans, (labor) => {
                    if (labor[constants.localIdKey] === tracker.laborlocalid) {
                        laborCache = labor;
                    }
                });
            });
        });

        function trackStartedLabor(parentId, labor) {
            parentIdCache = parentId;
            laborCache = labor;
            return dao.instantiate("ActiveLaborTracker", { parentid: parentIdCache, laborlocalid: labor[constants.localIdKey] }).then(tracker => dao.save(tracker));
        }

        function clearTrackedLabor() {
            return dao.executeQuery("delete from ActiveLaborTracker").then(() => {
                parentIdCache = null;
                laborCache = null;
            });
        }

        function calculateLineCost(regularhours, payrate) {
            const calcHours = !regularhours ? 0 : parseFloat(regularhours);
            const calcRate = !payrate ? 0 : parseFloat(payrate);
            const linecost = calcHours * calcRate;
            return _.isNaN(linecost) ? 0 : truncateDecimal(linecost);
        }

        const getActiveLabor = () => laborCache;

        const getActiveLaborParent = () => parentIdCache;

        const hasActiveLabor = () => !!getActiveLabor();

        function hasActiveLaborForCurrent() {
            const parent = crudContextService.currentDetailItem();
            return !parent || !parent.id || !hasActiveLabor()
                ? false
                : parent.id === getActiveLaborParent();
        }

        const getLabTransDetailSchema = () => crudContextService.currentCompositionSchemaById("labtrans", "detail");

        const getLabTransMetadata = () => crudContextService.currentCompositionTabByName("labtrans");

        function setInitialLaborAndCraft(datamap, overrideRegularHours) {
            const currentUser = securityService.currentFullUser();
            return dao.findSingleByQuery("AssociationData", `application = 'labor' and datamap like '%"personid":"${currentUser.PersonId}"%'`)
                .then(association => {
                    if (!association) return $q.reject(new Error(`There is no labor registered for the current user with personid '${currentUser.PersonId}'`));

                    const labor = association.datamap;
                    datamap["laborcode"] = labor.laborcode;
                    datamap["labor_.worksite"] = labor.worksite;
                    datamap["labor_.orgid"] = labor.orgid;

                    return dao.findSingleByQuery("AssociationData", `application = 'laborcraftrate' and datamap like '%"laborcode":"${labor.laborcode}"%'`);
                })
                .then(association => {
                    if (!association) return $q.reject(new Error(`There is no laborcraftrate registered for the labor '${datamap["laborcode"]}'`));

                    const craft = association.datamap;
                    datamap["craft"] = craft.craft;
                    const payrate = craft.rate;
                    datamap["payrate"] = payrate;

                    if (angular.isNumber(overrideRegularHours) && overrideRegularHours >= 0) {
                        datamap["regularhrs"] = overrideRegularHours;
                    }

                    datamap["linecost"] = calculateLineCost(datamap["regularhrs"], payrate);

                    return datamap;
                })
                .catch(error =>
                    $ionicPopup.alert({ title: "Labor Reporting Error", template: error.message }).then(() => $q.reject(error))
                );
        }

        function saveLabor(parent, labor, inCurrentParent, saveCustomMessage) {
            const application = crudContextService.currentApplicationName();
            const laborMetadata = getLabTransMetadata();

            return offlineSaveService.addAndSaveComposition(application, parent, labor, laborMetadata, saveCustomMessage)
                .then(savedParent => {
                    const context = crudContextService.getCrudContext();
                    if (!!inCurrentParent) {
                        // update the current detail context
                        // context.originalDetailItemDatamap = savedParent.datamap;
                    } else {
                        // find the correct parent in the list and update it
                        const parentIndex = context.itemlist.findIndex(i => i.id === savedParent.id);
                        if (parentIndex >= 0) context.itemlist[parentIndex] = savedParent;
                    }
                    return labor;
                });
        }

        function doStartLaborTransaction() {
            const parent = crudContextService.currentDetailItem();

            const laborDetailSchema = getLabTransDetailSchema();
            const labor = { "_newitem#$": true };
            offlineSchemaService.fillDefaultValues(laborDetailSchema, labor, parent.datamap);

            return setInitialLaborAndCraft(labor, 0)
                .then(initialized => saveLabor(parent, initialized, true, "Labor Timer Started"))
                .then(saved => {
                    return trackStartedLabor(parent.id, saved).then(() => {
                        menuModelService.updateAppsCount();
                        $rootScope.$broadcast("sw.labor.start");
                        return saved;
                    });
                });
        }

        function doStopLaborTransaction(parent) {
            const labor = getActiveLabor();
            const startdate = new Date(labor["startdate"]);
            const hoursDelta = ((new Date().getTime() - startdate.getTime()) / (1000 * 60 * 60));
            const hours = truncateDecimal(hoursDelta); // truncating and rounding to have 2 decimal
            labor["regularhrs"] = hours;
            labor["linecost"] = calculateLineCost(hours, labor["payrate"]);

            const stopingOnCurrentParent = !parent;
            const realParent = parent || crudContextService.currentDetailItem();

            return saveLabor(realParent, labor, stopingOnCurrentParent, "Labor Timer Stopped").then(() => {
                return clearTrackedLabor().then(() => {
                    $rootScope.$broadcast("sw.labor.stop");
                    return labor;
                });
            });
        }

        //#endregion

        //#region Public methods

        function shouldAllowLaborStart() {
            return !hasActiveLaborForCurrent();
        }

        function shouldAllowLaborFinish() {
            return hasActiveLaborForCurrent();
        }

        function showLaborCreationCommand() {
            return crudContextService.addCompositionAllowed() && shouldAllowLaborStart();
        }

        function showLaborFinishCommand() {
            return crudContextService.addCompositionAllowed() && shouldAllowLaborFinish();
        }

        function startLaborTransactionWhenLaborAlreadyStarted(parent) {
            return $ionicPopup.confirm({
                title: "Labor Reporting",
                template: "There's a labor timer started. Would you like to stop it in order to start a new one?"
            }).then(res => {
                if (res) {
                    // user wants to stop the current: stop it then start a new one
                    return doStopLaborTransaction(parent).then(() => doStartLaborTransaction());
                }
                // user does not wish to stop the previous: do nothing
                return null;
            });
        }

        /**
         * Starts a labor reporting/transaction on the current parent entity.
         * If there's a labor already in-progress on another work order it will ask if the user wishes to stop it.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<Datamap>} started labor
         */
        function startLaborTransaction(schema, datamap) {
            if (!shouldAllowLaborStart()) throw new Error("IllegalStateError: there's already an active labor transaction for the current item");

            if (!hasActiveLabor()) {
                return doStartLaborTransaction();
            }

            const parentId = getActiveLaborParent();
            return dao.findById("DataEntry", parentId).then((parent) => {
                // This is the case when a labor is started and for some reason the parent of the labor is not on db anymore
                // (after a sync that forces it not shown anymore or was a created one and deleted)
                // TODO: Add a alert on all cases that causes this to let user choose between finishing the labor or discard it
                if (!parent) {
                    $log.get("laborService#saveLabor").warn("Parent labor not found! This is the case when a labor timer is started and for some reason the parent of the labor is not on db anymore.");
                    return clearTrackedLabor().then(() => {
                        return doStartLaborTransaction();
                    });
                }
                return startLaborTransactionWhenLaborAlreadyStarted(parent);
            });
        }

        /**
         * Finishes an in-progress labor reporting/transaction on the current parent entity.
         * 
         * @param {} schema 
         * @param {} datamap 
         * @returns {} 
         */
        function finishLaborTransaction(schema, datamap) {
            if (!shouldAllowLaborFinish()) throw new Error("IllegalStateError: there's no active labor transaction for the current item");

            return $ionicPopup.confirm({
                title: "Active Labor Report",
                template: "Are you sure you want to stop the labor timer?"
            })
            .then(res =>  res ? doStopLaborTransaction() : null);
        }

        function finishLaborTransactionFromComposition(schema, datamap) {
            finishLaborTransaction(schema, datamap).then(result => {
                if (result) {
                    $rootScope.$broadcast("sw_updatecommandbar", "mobile.composition");
                }
            });
        }

        /**
         * Executed when detail schema is loaded:
         * - formats 'regularhrs' as hh.mm
         * - if it's a creation schema will set laborer, payrate and linecost information.  
         * 
         * @param {$scope} scope 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         */
        function onDetailLoad(scope, schema, datamap) {
            const regularHours = datamap["regularhrs"];
            if (angular.isNumber(regularHours) && !Number.isInteger(regularHours)) {
                datamap["regularhrs"] = truncateDecimal(regularHours);
            }
            if (!datamap["labtransid"]) {
                setInitialLaborAndCraft(datamap);
            }
        }

        /**
         * Updates linecost when regularhrs changes.
         * 
         * @param {events.afterchange} event 
         */
        function updateLineCost(event) {
            const datamap = event.datamap;
            datamap["linecost"] = calculateLineCost(datamap["regularhrs"], datamap["payrate"]);
        }

        /**
         * Formats the 'rugularhrs' field as `HHh MMm {SSs}`. 
         * 
         * @param {formatter.params} params 
         * @returns {String} 
         */
        function formatRegularHours(params) {
            const value = params.value;
            if (!value) return value;
            const hours = Math.trunc(value);
            const minutes = Math.round((value * 60) % 60);
            if (hours === 0 && minutes === 0) {
                const seconds = Math.round((value * 3600) % 3600);
                return `${hours}h ${minutes}m ${seconds}s`;
            }
            return `${hours}h ${minutes}m`;
        }

        /**
         * Formats 'genapprservreceipt' boolean field to "Yes" and "No".
         * 
         * @param {formatter.params} params 
         * @returns {String} 
         */
        function formatApproved(params) {
            const value = params.value;
            if (!value) return value;
            const approved = _.contains([true, "true", "True", 1, "1", "yes", "Yes"], value);
            return approved ? "Yes" : "No";
        }

        /**
         * Clears the labor cache if the given item is the current labor parent
         * @param {} item 
         * @returns {} 
         */
        function clearLaborCacheIfCurrentParent(item) {
            if (item.id === getActiveLaborParent()) {
                clearTrackedLabor();
            }
        }

        function confirmPossibleTimer(item, defaultPreDeleteAction) {
            const activeLabor = getActiveLabor();
            return !!activeLabor && item["#localswdbid"] === activeLabor["#localswdbid"]
                ? $ionicPopup.confirm({
                    title: "Delete Labor",
                    template: "The labor you are trying to delete has a timer. Are you sure you wish to cancel the timer and delete it?"
                })
                : defaultPreDeleteAction();
        }


        function cancelPossibleTimer(item, parent, defaultPostDeleteAction) {
            const activeLabor = getActiveLabor();
            if (!!activeLabor && item["#localswdbid"] === activeLabor["#localswdbid"]) {
                clearTrackedLabor();
                return parent;
            }
            return defaultPostDeleteAction();
        }

        function hasItemActiveLabor(item) {
            if (!item) {
                return false;
            }

            const activeLabor = getActiveLabor();

            //is labor composition item
            if (!item.application && !!activeLabor) {
                return activeLabor["#localswdbid"] === item["#localswdbid"];
            }

            return getActiveLaborParent() === item.id;
        }

        //#endregion

        //#region Service Instance
        const service = {
            onDetailLoad,
            shouldAllowLaborStart,
            shouldAllowLaborFinish,
            showLaborCreationCommand,
            showLaborFinishCommand,
            startLaborTransaction,
            finishLaborTransaction,
            finishLaborTransactionFromComposition,
            updateLineCost,
            formatRegularHours,
            formatApproved,
            getActiveLaborParent,
            getActiveLabor,
            confirmPossibleTimer,
            cancelPossibleTimer,
            hasItemActiveLabor,
            clearLaborCacheIfCurrentParent
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_mobile_services")
        .factory("laborService",
        ["swdbDAO", "securityService", "localStorageService", "crudContextService", "$ionicPopup", "$q", "$log", "offlineSchemaService", "offlineSaveService", "$rootScope", "menuModelService", laborService]);
    //#endregion

})(angular, _);