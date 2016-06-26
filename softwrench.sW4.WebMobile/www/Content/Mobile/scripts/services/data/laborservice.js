(function (angular, _) {
    "use strict";

    function laborService(dao, securityService, localStorageService, crudContextService, $ionicPopup, $q, offlineSchemaService, offlineSaveService) {
        //#region Utils

        const constants = {
            cache: {
                laborKey: "sw:labor:activelabor",
                laborParentKey: "sw:labor:activelabor:parent"
            }
        };

        function cacheStartedLabor (parentId, labor) {
            localStorageService.put(constants.cache.laborParentKey, parentId);
            localStorageService.put(constants.cache.laborKey, labor);
        }

        function clearCachedLabor() {
            localStorageService.remove(constants.cache.laborParentKey);
            localStorageService.remove(constants.cache.laborKey);
        }

        function calculateLineCost(regularhours, payrate) {
            const calcHours = !regularhours ? 0 : parseFloat(regularhours);
            const calcRate = !payrate ? 0 : parseFloat(payrate);
            const linecost = calcHours * calcRate;
            return _.isNaN(linecost) ? 0 : linecost;
        }

        const getActiveLabor = () => localStorageService.get(constants.cache.laborKey);

        const getActiveLaborParent = () => localStorageService.get(constants.cache.laborParentKey);

        const hasActiveLabor = () => !!getActiveLabor();

        function hasActiveLaborForCurrent() {
            const parent = crudContextService.currentDetailItem();
            return !parent || !parent.id || !hasActiveLabor()
                ? false
                : parent.id === getActiveLaborParent();
        }

        const getLabTransDetailSchema = () => crudContextService.currentCompositionSchemaById("labtrans", "detail");
        
        const getLabTransMetadata = () => crudContextService.currentCompositionTabByName("labtrans");

        function setInitialLaborAndCraft(datamap) {
            const currentUser = securityService.currentFullUser();
            return dao.findSingleByQuery("AssociationData", `application = 'labor' and datamap like '%"personid":"${currentUser.PersonId}"%'`)
                .then(association => {
                    const labor = association.datamap;
                    datamap["laborcode"] = labor.laborcode;
                    datamap["labor_.worksite"] = labor.worksite;
                    datamap["labor_.orgid"] = labor.orgid;

                    return dao.findSingleByQuery("AssociationData", `application = 'laborcraftrate' and datamap like '%"laborcode":"${labor.laborcode}"%'`);
                })
                .then(association => {
                    const craft = association.datamap;
                    datamap["craft"] = craft.craft;
                    const payrate = craft.rate;
                    datamap["payrate"] = payrate;

                    datamap["linecost"] = calculateLineCost(datamap["regularhrs"], payrate);

                    return datamap;
                });
        }

        function saveLabor(parent, labor, inCurrentParent) {
            const application = crudContextService.currentApplicationName();
            const laborMetadata = getLabTransMetadata();

            return offlineSaveService.addAndSaveComposition(application, parent, labor, laborMetadata)
                .then(savedParent => {
                    const context = crudContextService.getCrudContext();
                    if (!!inCurrentParent) {
                        // update the current detail context
                        context.originalDetailItemDatamap = savedParent.datamap;
                    } else {
                        // find the correct parent in the list and update it
                        const parentIndex = context.itemlist.findIndex(i => i.id === savedParent.id);
                        if(parentIndex >= 0) context.itemlist[parentIndex] = savedParent;
                    }
                    return labor;
                });
        }

        function doStartLaborTransaction() {
            const parent = crudContextService.currentDetailItem();

            const laborDetailSchema = getLabTransDetailSchema();
            const labor = { "_newitem#$": true };
            offlineSchemaService.fillDefaultValues(laborDetailSchema, labor, parent.datamap);

            return setInitialLaborAndCraft(labor)
                .then(initialized => saveLabor(parent, initialized, true))
                .then(saved => {
                    cacheStartedLabor(parent.id, saved);
                    return saved;
                });
        }

        function doStopLaborTransaction(parentId) {
            const labor = getActiveLabor();
            const startdate = new Date(labor["startdate"]);
            const hours = ((new Date().getTime() - startdate.getTime()) / (1000 * 60 * 60));
            labor["regularhrs"] = hours;
            labor["linecost"] = calculateLineCost(hours, labor["payrate"]);

            const stopingOnCurrentParent = !parentId;
            const parentPromise = stopingOnCurrentParent
                ? $q.when(crudContextService.currentDetailItem())
                : dao.findById("DataEntry", parentId);

            return parentPromise
                .then(parent => saveLabor(parent, labor, stopingOnCurrentParent))
                .then(() => {
                    clearCachedLabor();
                    return labor;
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

            return hasActiveLabor()
                // another labor already in progress: ask user input
                ? $ionicPopup.confirm({
                    title: "Labor Reporting",
                    template: "There's a labor reporting in progress. Would you like to stop it in order to start a new one ?"
                })
                .then(res => {
                    if (res) {
                        // user wants to stop the current: stop it then start a new one
                        const parentOfCurrentActive = getActiveLaborParent();
                        return doStopLaborTransaction(parentOfCurrentActive).then(() => doStartLaborTransaction());
                    }
                    // user does not wish to stop the previous: do nothing
                    return null;
                })
                // no labor in progress: just start a new one
                : doStartLaborTransaction();
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
                template: "Are you sure you want to finish the current labor report ?"
            })
            .then(res =>  res ? doStopLaborTransaction() : null);
        }

        function onDetailLoad(scope, schema, datamap) {
            if (!!datamap["labtransid"]) return;
            setInitialLaborAndCraft(datamap);
        }

        function updateLineCost(event) {
            const datamap = event.datamap;
            datamap["linecost"] = calculateLineCost(datamap["regularhrs"], datamap["payrate"]);
        }

        //#endregion

        //#region Service Instance
        const service = {
            onDetailLoad,
            shouldAllowLaborStart,
            shouldAllowLaborFinish,
            startLaborTransaction,
            finishLaborTransaction,
            updateLineCost
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_mobile_services")
        .factory("laborService",
        ["swdbDAO", "securityService", "localStorageService", "crudContextService", "$ionicPopup", "$q", "offlineSchemaService", "offlineSaveService", laborService]);
    //#endregion

})(angular, _);