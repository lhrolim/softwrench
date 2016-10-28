﻿(function (mobileServices, angular, constants, _) {
    "use strict";
    constants = constants || {};

    mobileServices.factory('crudContextService', [
    "$q", "$log", "$rootScope", "swdbDAO", "searchIndexService", "problemService", 
    "metadataModelService", "offlineSchemaService", "offlineCompositionService", "expressionService",
    "offlineSaveService", "schemaService", "contextService", "routeService", "tabsService",
    "crudFilterContextService", "validationService", "crudContextHolderService", "datamapSanitizationService", "maximoDataService", "menuModelService", "loadingService", "offlineAttachmentService", "offlineEntities", "queryListBuilderService", "swAlertPopup", "eventService",
    function ($q, $log, $rootScope, dao, searchIndexService, problemService, 
    metadataModelService, offlineSchemaService, offlineCompositionService, expressionService,
    offlineSaveService, schemaService, contextService, routeService, tabsService,
    crudFilterContextService, validationService, crudContextHolderService, datamapSanitizationService, maximoDataService, menuModelService, loadingService, offlineAttachmentService, entities, queryListBuilderService, swAlertPopup, eventService) {

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

        return {

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
                            crudContext.originalDetailItemDatamap = angular.copy(datamap);
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

                return this.saveCurrentItem(showConfirmationMessage);
            },

            saveCurrentItem: function (showConfirmationMessage) {
                const crudContext = crudContextHolderService.getCrudContext();
                const applicationName = crudContext.currentApplicationName;
                const item = crudContext.currentDetailItem;
                const datamap = item.datamap;
                const schema = this.currentDetailSchema();

                const beforeSave = eventService.dispatch("offline.presave", schema, { schema, datamap });

                return $q.when(beforeSave)
                    .then(() => {
                        return offlineSaveService.saveItem(applicationName, item, showConfirmationMessage);
                    })
                    .then(saved => {
                        crudContext.originalDetailItemDatamap = angular.copy(item.datamap);
                        contextService.insertIntoContext("crudcontext", crudContext);
                        if (crudContext.newItem) {
                            crudContext.newItem = false;
                            return this.refreshGrid().then(() => saved);
                        }
                        return this.refreshIfLeftJoinPresent(crudContext, saved);
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

                const promise = dao.executeStatement(entities.DataEntry.deleteLocalStatement, [item.id, application]);

                return newAttachments.length > 0 
                    ? promise.then(() => offlineAttachmentService.deleteRelatedAttachments(newAttachments))
                    : promise;
            },

            //#endregion

            //#region GridFNS

            refreshGrid: function (skipPostFilter) {
                var crudContext = crudContextHolderService.getCrudContext();
                crudContext.itemlist = [];
                internalListContext.lastPageLoaded = 1;
                internalListContext.pageNumber = 1;
                return this.loadMorePromise().then(function () {
                    if (skipPostFilter) {
                        return $q.when();
                    }
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
                //appending root prefix, since a left join could be present leading to ambiguity amongst columns
                var baseQuery = "`root`.application = '{0}'".format(crudContext.currentApplicationName);
                if (quickSearch.value) {
                    baseQuery += ' and `root`.datamap like \'%:"{0}%\''.format(quickSearch.value);
                }
                if (!crudFilterContextService.showPending()) {
                    baseQuery += ' and `root`.pending = 0 ';
                }
                if (!crudFilterContextService.showDirty()) {
                    baseQuery += ' and `root`.isDirty = 0 ';
                }

                baseQuery += searchIndexService.buildSearchQuery(appName, listSchema, gridSearch);

                baseQuery += searchIndexService.buildSortQuery(appName, listSchema, gridSearch);

                let queryObj = { pagesize: 10, pageNumber: internalListContext.lastPageLoaded };

                queryObj = angular.extend(queryObj, queryListBuilderService.buildJoinParameters(listSchema));

                return dao.findByQuery("DataEntry", baseQuery, queryObj)
                    .then(function (results) {
                        internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                        for (var i = 0; i < results.length; i++) {
                            crudContext.itemlist.push(results[i]);
                        }
                        return $q.when(results);
                    });
            },


            loadApplicationGrid: function (applicationName, applicationTitle, schemaId) {
                if (lastGridApplication && lastGridApplication !== applicationName) {
                    crudContextHolderService.clearGridSearch();
                }
                lastGridApplication = applicationName;

                const crudContext = crudContextHolderService.getCrudContext(); //cleaning up
                crudContext.currentDetailItem = null;
                crudContext.composition = {};
                const application = metadataModelService.getApplicationByName(applicationName);
                crudContext.currentTitle = applicationTitle;
                crudContext.currentApplicationName = applicationName;
                crudContext.currentApplication = application;
                crudContext.newItem = false;


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
                crudContext.originalDetailItemDatamap = angular.copy(crudContext.currentDetailItem.datamap);
                crudContextHolderService.setPreviousAndNextItems(item);
                if (isRippleEmulator()) {
                    contextService.insertIntoContext("crudcontext", crudContext);
                }

                return problemService.getProblems(item.id).then(problems => {
                    crudContext.currentProblems = problems;
                    return routeService.go("main.cruddetail.maininput");
                }).then(() => {
                    $rootScope.$emit("sw_cruddetailrefreshed");
                    loadingService.hide();
                });
            }

            //#endregion
        }



    }]);

})(mobileServices, angular, constants, _);
