(function (mobileServices, angular, constants, _) {
    "use strict";
    constants = constants || {};

    mobileServices.factory('crudContextService', [
    "$q", "$log", "$rootScope", "$ionicHistory", "swdbDAO", "searchIndexService", "problemService",
    "metadataModelService", "offlineSchemaService", "offlineCompositionService", "expressionService",
    "offlineSaveService", "schemaService", "contextService", "routeService", "tabsService",
    "crudFilterContextService", "validationService", "crudContextHolderService", "datamapSanitizationService", "maximoDataService", "menuModelService", "loadingService", "offlineAttachmentService", "offlineEntities", "queryListBuilderService", "swAlertPopup", "eventService",
    function ($q, $log, $rootScope, $ionicHistory, dao, searchIndexService, problemService,
    metadataModelService, offlineSchemaService, offlineCompositionService, expressionService,
    offlineSaveService, schemaService, contextService, routeService, tabsService,
    crudFilterContextService, validationService, crudContextHolderService, datamapSanitizationService, maximoDataService, menuModelService, loadingService, offlineAttachmentService, entities, queryListBuilderService, swAlertPopup, eventService) {

        let service = {};

        // ReSharper disable once InconsistentNaming
        var internalListContext = {
            lastPageLoaded: 1
        }

        $rootScope.$on("sw4:security:logout", function () {
            metadataModelService.reset();
            crudContextHolderService.reset();
            menuModelService.reset();
        });

        // used to know when to clear search and sort data structure
        var lastGridApplication = null;

        const afterSaveNew = function (newItem, crudContext) {
            crudContext.originalDetailItemDatamap = angular.copy(newItem.datamap);
            return service.refreshGrid(true).then(() => {
                $rootScope.$broadcast("sw_cruddetailrefreshed");
                return newItem;
            });
        }

        service = {
            //#region delegateMethods
            getCrudContext: function () {
                return crudContextHolderService.getCrudContext();
            },

            restoreState: function () {
                const savedState = crudContextHolderService.restoreState();
                if (savedState && savedState.itemlist && !savedState.currentDetailItem) {
                    this.refreshGrid();
                }
                return savedState;
            },

            resetContext: function () {
                crudContextHolderService.reset();
            },

            isList: function () {
                return crudContextHolderService.isList();
            },

            currentTitle: function () {
                return crudContextHolderService.currentTitle();
            },

            currentApplicationName: function () {
                return crudContextHolderService.currentApplicationName();
            },

            currentListSchema: function () {
                return crudContextHolderService.currentListSchema();
            },

            currentDetailSchema: function () {
                return crudContextHolderService.currentDetailSchema();
            },

            currentDetailItem: function () {
                return crudContextHolderService.currentDetailItem();
            },

            itemlist: function () {
                return crudContextHolderService.itemlist();
            },

            currentDetailItemDataMap: function () {
                return crudContextHolderService.currentDetailItemDataMap();
            },

            leavingDetail: function () {
                crudContextHolderService.leavingDetail();
            },

            isOnMainTab: function () {
                return crudContextHolderService.isOnMainTab();
            },

            resetTab: function () {
                crudContextHolderService.resetTab();
            },

            tabTitle: function () {
                return crudContextHolderService.tabTitle();
            },

            tabIcon: function () {
                return crudContextHolderService.tabIcon();
            },

            leavingCompositionDetail: function () {
                crudContextHolderService.leavingCompositionDetail();
            },

            //#region composition
            compositionList: function () {
                return crudContextHolderService.compositionList();
            },

            getCompositionListSchema: function () {
                return crudContextHolderService.getCompositionListSchema();
            },

            getCompositionDetailSchema: function () {
                return crudContextHolderService.getCompositionDetailSchema();
            },

            getCompositionDetailItem: function () {
                return crudContextHolderService.getCompositionDetailItem();
            },
            //#endregion

            //#endregion


            mainDisplayables: function () {
                return schemaService.nonTabFields(this.currentDetailSchema());
            },


            //#region Compositions

            currentCompositionsToShow: function () {
                const detailSchema = this.currentDetailSchema();
                if (!detailSchema) {
                    return [];
                }
                const allDisplayables = tabsService.tabsDisplayables(detailSchema);
                return allDisplayables;
            },

            currentCompositionTabByName: function (composition) {
                return this.currentCompositionsToShow().find(c => c.attribute === composition);
            },

            currentCompositionSchemaById: function (composition, schemaId) {
                const compositionMetadata = this.currentCompositionTabByName(composition);
                return !compositionMetadata ? null : compositionMetadata.schema.schemas[schemaId];
            },

            loadTab: function (tab) {
                var crudContext = crudContextHolderService.getCrudContext();
                if (tab == null) {
                    //let´s return to the main tab
                    this.resetTab();
                    return routeService.go("main.cruddetail.maininput");
                }

                if (tab.type !== "ApplicationCompositionDefinition") {
                    //tabs do not need to load from the database since the data is already contained on the main datamap
                    crudContext.composition.currentTab = tab;
                    return routeService.go("main.cruddetail.compositionlist");
                }

                return offlineCompositionService.loadCompositionList(crudContext.currentDetailItem, tab).then(function (compositionItems) {
                    crudContext.composition.currentTab = tab;
                    crudContext.composition.itemlist = compositionItems;
                    crudContext.composition.currentListSchema = tab.schema.schemas.list;
                    crudContext.composition.currentDetailSchema = tab.schema.schemas.detail;
                    crudContext.composition.currentDetailItem = null;
                    contextService.insertIntoContext("crudcontext", crudContext);
                    return routeService.go("main.cruddetail.compositionlist");
                });
            },

            //TODO: move to offlinecompositionservice perhaps?
            loadCompositionDetail: function (item) {
                const errors = this.validateDetail();
                if (errors && errors.length > 0) {
                    // main form has validations: disable edit and redirect to main tab
                    return swAlertPopup.alertValidationErrors(errors, null, `Validation errors in the main form prevent you from editing this ${this.tabTitle()}. Please resolve this errors:`)
                        .then(() => this.loadTab());
                }
                const crudContext = crudContextHolderService.getCrudContext();
                const compositionDetailSchema = this.getCompositionDetailSchema();
                const fields = compositionDetailSchema.displayables;
                if (compositionDetailSchema.applicationName === "attachment") {
                    return offlineAttachmentService.loadRealAttachment(item);
                }
                datamapSanitizationService.enforceNumericType(item, fields);
                //for compositions item will be the datamap itself
                crudContext.composition.currentDetailItem = item;
                crudContext.composition.originalDetailItemDatamap = angular.copy(item);
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.compositiondetail");
            },

            addCompositionAllowed: function() {
                const context = crudContextHolderService.getCrudContext();
                const composition = context.composition;
                if (!composition || !composition.currentTab || !composition.currentTab.schema || composition.currentDetailItem != null) {
                    return false;
                }

                const allowInsertion = composition.currentTab.schema.allowInsertion;
                const datamap = context.currentDetailItem.datamap;
                const schema = context.currentDetailSchema;
                return expressionService.evaluate(allowInsertion, datamap, { schema: schema }, null);
            },

            createNewCompositionItem: function () {
                const errors = this.validateDetail();
                if (errors && errors.length > 0) {
                    // main form has validations: disable create/add and redirect to main tab
                    return swAlertPopup.alertValidationErrors(errors, null, `Validation errors in the main form prevent you from adding a new ${this.tabTitle()}. Please resolve this errors:`)
                        .then(() => this.loadTab());
                }
                const crudContext = crudContextHolderService.getCrudContext();
                const compositionParentDatamap = crudContext.currentDetailItem.datamap;
                crudContext.composition.currentDetailItem = {};
                offlineSchemaService.fillDefaultValues(crudContext.composition.currentDetailSchema, crudContext.composition.currentDetailItem, compositionParentDatamap);
                crudContext.composition.originalDetailItemDatamap = {
                    //to make this new item always dirty!!!
                    "_newitem#$": true
                };
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.compositiondetail");
            },


            //#endregion

            //#region saveFNS

            hasDirtyChanges: function () {
                return crudContextHolderService.hasDirtyChanges();
            },

            cancelChanges: function () {
                const crudContext = crudContextHolderService.getCrudContext();
                if (crudContext.composition.currentDetailItem) {
                    crudContext.composition.currentDetailItem = angular.copy(crudContext.composition.originalDetailItemDatamap);
                    return routeService.go("main.cruddetail.compositionlist");
                }
                if (crudContext.newItem) {
                    this.refreshGrid();
                    crudContext.newItem = false;
                } else {
                    crudContext.currentDetailItem.datamap = angular.copy(crudContext.originalDetailItemDatamap);
                }

            },

            validateDetail: function (crudForm, schemaToValidate, displayables) {
                const crudContext = crudContextHolderService.getCrudContext();
                crudForm = crudForm || {};
                const detailSchema = schemaToValidate || this.currentDetailSchema();
                const datamap = crudContext.currentDetailItem.datamap;
                const toValidateDisplayables = displayables || detailSchema.displayables;
                return validationService.validate(detailSchema, toValidateDisplayables, datamap, crudForm.$error);
            },

            saveChanges: function (crudForm, showConfirmationMessage) {
                const crudContext = crudContextHolderService.getCrudContext();
                crudForm = crudForm || {};
                const validationErrors = this.validateDetail(crudForm);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return $q.reject(validationErrors);
                }

                const datamap = crudContext.currentDetailItem.datamap;
                const composition = crudContext.composition;
                if (composition && composition.currentDetailItem) {
                    const compositionItem = composition.currentDetailItem;
                    const validationErrors = validationService.validate(composition.currentDetailSchema, composition.currentDetailSchema.displayables, compositionItem, crudForm.$error);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return $q.reject(validationErrors);
                    }

                    return offlineSaveService.addAndSaveComposition(crudContext.currentApplicationName, crudContext.currentDetailItem, compositionItem, composition.currentTab)
                        .then(() => {
//                            crudContext.originalDetailItemDatamap = angular.copy(datamap);
                            composition.originalDetailItemDatamap = composition.currentDetailItem;
                            return this.refreshIfLeftJoinPresent(crudContext, null);
                        })
                        .then(saved => {
                            const compositionItemInList = composition.itemlist.find(c => c[constants.localIdKey] === compositionItem[constants.localIdKey]);
                            if (!compositionItemInList) {
                                return saved;
                            }
                            angular.forEach(compositionItemInList, (value, key) => {
                                if (!compositionItem.hasOwnProperty(key) || key === "$$hashKey") return;
                                compositionItemInList[key] = compositionItem[key];
                            });
                            return saved;
                        })
                        .then(saved => this.loadTab(composition.currentTab).then(() => saved));
                }

                return this.saveCurrentItem(showConfirmationMessage, true);
            },



            saveCurrentItem: function (showConfirmationMessage, loadSavedItem) {
                const crudContext = crudContextHolderService.getCrudContext();
                const applicationName = crudContext.currentApplicationName;
                const item = crudContext.currentDetailItem;
                const datamap = item.datamap;
                const schema = this.currentDetailSchema();

                const beforeSave = eventService.dispatch("offline.presave", schema, { schema, datamap });

                return $q.when(beforeSave)
                    .then(() => {
                        const title = crudContext.currentApplication.data.title || applicationName;
                        return offlineSaveService.saveItem(applicationName, item, title, showConfirmationMessage);
                    })
                    .then(saved => {
                        contextService.insertIntoContext("crudcontext", crudContext);
                        if (!crudContext.newItem) {
//                            crudContext.originalDetailItemDatamap = angular.copy(saved.datamap);
                            return this.refreshIfLeftJoinPresent(crudContext, saved);
                        }

                        menuModelService.updateAppsCount();
                        crudContext.newItem = false;

                        if (!loadSavedItem) {
                            return afterSaveNew(saved, crudContext);
                        }

                        const listSchema = crudContextHolderService.currentListSchema();
                        const joinObj = queryListBuilderService.buildJoinParameters(listSchema);
                        const qry = "`root`.application = '{0}' and `root`.id = '{1}'".format(applicationName, saved.newId);
                        return dao.findByQuery("DataEntry", qry, joinObj).then((results) => {
                            const newItem = results[0];
                            crudContext.currentDetailItem = newItem;
                            return afterSaveNew(newItem, crudContext);
                        });
                    });
            },
            
            refreshIfLeftJoinPresent: function (crudContext, saved) {
                const itemlist = this.itemlist();
                const currentDetailItem = crudContext.currentDetailItem;
                if (itemlist.length > 0) {
                    //due to possible leftjoins
                    const repeatedItems = itemlist.some(i => i.id === currentDetailItem.id && i.generatedRowStamp !== currentDetailItem.generatedRowStamp);
                    if (repeatedItems) {
                        //let´s garantee that all items with the same id are updated (eventual left joins)
                        return this.refreshGrid(true).then(() => saved);
                    }
                }
                return saved;
            },

            restoreItemToOriginalState: function (item) {
                const crudContext = crudContextHolderService.getCrudContext();
                const application = crudContext.currentApplicationName;

                const newAttachments = (() => {
                    const originalAttachments = item.originaldatamap["attachment_"] || [];
                    const allAttachments = item.datamap["attachment_"] || [];

                    if (allAttachments.length <= 0) return [];

                    const allHashes = allAttachments.map(a => a["#offlinehash"]);
                    const originalHashes = originalAttachments.map(a => a["#offlinehash"]);
                    const newHashes = _.difference(allHashes, originalHashes);

                    return allAttachments.filter(a => _.contains(newHashes, a["#offlinehash"]));
                })();

                const hadProblem = item.hasProblem;
                item.isDirty = false;
                item.hasProblem = false;

                var promise = dao.executeStatement(entities.DataEntry.restoreToOriginalStateStatement, [item.id, application]);
                if (hadProblem) {
                    item.hasProblem = false;
                    promise = promise.then(() => problemService.deleteRelatedProblems(item.id));
                }
                if (newAttachments.length > 0) {
                    promise = promise.then(() => offlineAttachmentService.deleteRelatedAttachments(newAttachments));
                }

                return promise;
            },

            deleteLocalItem: function(item) {
                const crudContext = crudContextHolderService.getCrudContext();
                const application = crudContext.currentApplicationName;

                const newAttachments = item.datamap["attachment_"] || [];

                const promise = dao.executeStatement(entities.DataEntry.deleteLocalStatement, [item.id, application]).then((result) => {
                    menuModelService.updateAppsCount();
                    return result;
                });

                return newAttachments.length > 0 
                    ? promise.then(() => offlineAttachmentService.deleteRelatedAttachments(newAttachments))
                    : promise;
            },

            //#endregion

            //#region GridFNS

            refreshGrid: function (skipPostFilter) {
                const log = $log.get("crudContextService#refreshGrid", ["list", "crud"]);
                var crudContext = crudContextHolderService.getCrudContext();
                crudContext.itemlist = [];
                internalListContext.lastPageLoaded = 1;
                internalListContext.pageNumber = 1;
                return this.loadMorePromise().then(function () {
                    if (skipPostFilter) {
                        return $q.when();
                    }
                    log.debug("application grid refreshed");
                    contextService.insertIntoContext("crudcontext", crudContext);
                    return routeService.go("main.crudlist");
                });
            },


            loadMorePromise: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                const gridSearch = crudContextHolderService.getGridSearchData();
                const quickSearch = crudContextHolderService.getQuickSearch();
                const listSchema = crudContextHolderService.currentListSchema();
                const appName = crudContextHolderService.currentApplicationName();

                let extraWhereClause = "1=1";
                if (quickSearch.value) {
                    extraWhereClause += ' and `root`.datamap like \'%:"{0}%\''.format(quickSearch.value);
                }

                extraWhereClause += searchIndexService.buildSearchQuery(appName, listSchema, gridSearch);

                let baseQuery = menuModelService.buildListQuery(crudContext.currentApplicationName, crudContext.currentMenuId, extraWhereClause);

                const joinObj = queryListBuilderService.buildJoinParameters(listSchema);

                if (internalListContext.lastPageLoaded === 1) {
                    const countQuery = baseQuery;
                    dao.countByQuery("DataEntry", countQuery, joinObj).then((count) => {
                        gridSearch.count = count;
                    });
                }

                baseQuery += searchIndexService.buildSortQuery(appName, listSchema, gridSearch);

                const queryObj = angular.extend({ pagesize: 10, pageNumber: internalListContext.lastPageLoaded }, joinObj);

                return dao.findByQuery("DataEntry", baseQuery, queryObj)
                    .then(function (results) {
                        internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                        for (var i = 0; i < results.length; i++) {
                            crudContext.itemlist.push(results[i]);
                        }
                        return $q.when(results);
                    });
            },


            loadApplicationGrid: function (applicationName, schemaId, menuId, menuParams) {
                const log = $log.get("crudContextService#loadApplicationGrid", ["list", "crud"]);
                log.debug("loading application grid");
                if (lastGridApplication && lastGridApplication !== applicationName) {
                    crudContextHolderService.clearGridSearch();
                }
                lastGridApplication = applicationName;

                const crudContext = crudContextHolderService.getCrudContext(); //cleaning up
                crudContext.currentDetailItem = null;
                crudContext.composition = {};
                const application = metadataModelService.getApplicationByName(applicationName);
                crudContext.currentTitle = application.data.title;
                crudContext.currentApplicationName = applicationName;
                crudContext.currentApplication = application;
                crudContext.newItem = false;

                if (menuId) {
                    crudContext.currentMenuId = menuId;
                    crudContext.menuGridTitle = menuParams && menuParams.offlinegridtitle;
                    crudContext.menuDisableCreate = menuParams && menuParams.offlineDisableCreate === "true";
                }

                crudContext.currentListSchema = offlineSchemaService.locateSchema(application, schemaId);
                crudContext.currentDetailSchema = offlineSchemaService.loadDetailSchema(crudContext.currentListSchema, crudContext.currentApplication);
                crudContext.currentNewDetailSchema = offlineSchemaService.locateSchemaByStereotype(crudContext.currentApplication, "detailnew");
                if (crudContext.currentNewDetailSchema == null && schemaService.isPropertyTrue(crudContext.currentDetailSchema, "mobile.actasnewschema")) {
                    //if this property is true, then the detail schema will also be used as the newschema
                    crudContext.currentNewDetailSchema = crudContext.currentDetailSchema;
                }
                return this.refreshGrid();
            },

            hasNewSchemaAvailable: function () {
                const crudContext = crudContextHolderService.getCrudContext();
                const newDetailSchema = crudContext.currentNewDetailSchema;
                return !!newDetailSchema;
            },

            //#endregion

            //#region detailFNs

            navigatePrevious: function () {
                const crudContext = crudContextHolderService.getCrudContext();
                if (!crudContext.previousItem) {
                    return routeService.go("main.crudlist");
                } else {
                    return this.loadDetail(crudContext.previousItem);
                }

            },

            navigateNext: function () {
                const crudContext = crudContextHolderService.getCrudContext();
                if (!crudContext.nextItem) {
                    return this.loadMorePromise().then(results => {
                        if (!results || results.length <= 0) {
                            //end has reached;
                            return $q.when();
                        }
                        crudContextHolderService.setPreviousAndNextItems(crudContext.currentDetailItem);
                        if (!!crudContext.nextItem) {
                            return this.loadDetail(crudContext.nextItem);
                        }
                        return $q.when();
                    });
                }
                return this.loadDetail(crudContext.nextItem);
            },
            
            isCreation: function () {
                const crudContext = this.getCrudContext();
                return !crudContext.originalDetailItemDatamap || crudContext.originalDetailItemDatamap["_newitem#$"];
            },

            gridTitle: function(gridSchema) {
                if (!gridSchema || !gridSchema.title) {
                    return this.currentTitle();
                }
                return gridSchema.title;
            },

            createDetail: function () {
                const crudContext = crudContextHolderService.getCrudContext();
                crudContext.wizardStateIndex = 0;
                crudContext.currentDetailItem = {
                    datamap: {}
                };
                crudContext.newItem = true;
                // TODO: add support for schema registered hooks
                offlineSchemaService.fillDefaultValues(this.currentDetailSchema(), crudContext.currentDetailItem.datamap);
                crudContext.originalDetailItemDatamap = {
                    //to make this new item always dirty!!!
                    "_newitem#$": true
                };
                return routeService.go("main.cruddetail.maininput");
            },

            loadDetailByMaximoUid: function (application, schema, refId) {
                var that = this;
                maximoDataService.loadItemByMaximoUid(application, schema, refId)
                    .then(function (item) {
                        return that.loadDetail(item);
                    });
            },

           

            loadDetail: function (item) {
                return $ionicHistory.clearCache().then(() => {
                    const log = $log.get("crudContextService#loadDetail", ["crud", "detail"]);
                    log.debug("load detail init");
                    loadingService.showDefault();
                    const crudContext = crudContextHolderService.getCrudContext(); /// <summary>
                    ///  Loads a detail represented by the parameter item.
                    /// </summary>
                    /// <param name="item"></param>
                    /// <returns type=""></returns>
                    if (!crudContext.currentDetailSchema) {
                        crudContext.currentDetailSchema = offlineSchemaService.loadDetailSchema(crudContext.currentListSchema, crudContext.currentApplication, item);
                    }
                    const fields = this.mainDisplayables();
                    datamapSanitizationService.enforceNumericType(item.datamap, fields);

                    crudContext.currentDetailItem = item;
                    eventService.onload({}, crudContext.currentDetailSchema, crudContext.currentDetailItem.datamap, {});
                    crudContext.originalDetailItemDatamap = angular.copy(crudContext.currentDetailItem.datamap);
                    crudContextHolderService.setPreviousAndNextItems(item);
                    if (isRippleEmulator()) {
                        contextService.insertIntoContext("crudcontext", crudContext);
                    }
                    log.debug("loading problems");
                    return problemService.getProblems(item.id).then(problems => {
                        log.debug("problems loaded done");
                        crudContext.currentProblems = problems;
                        return routeService.go("main.cruddetail.maininput");
                    }).then(() => {
                        log.debug("crud detail finished loading");
                        $rootScope.$broadcast("sw_cruddetailrefreshed");
                        loadingService.hide();
                    });
                });
            }
            //#endregion
        }

        return service;

    }]);

})(mobileServices, angular, constants, _);
