(function (mobileServices, angular, constants) {
    "use strict";
    constants = constants || {};

    mobileServices.factory('crudContextService', [
    "$q", "$log", "$rootScope", "swdbDAO", "searchIndexService",
    "metadataModelService", "offlineSchemaService", "offlineCompositionService",
    "offlineSaveService", "schemaService", "contextService", "routeService", "tabsService",
    "crudFilterContextService", "validationService", "crudContextHolderService", "datamapSanitizationService", "maximoDataService", "menuModelService", "loadingService", "offlineAttachmentService",
    function ($q, $log, $rootScope, swdbDAO, searchIndexService,
    metadataModelService, offlineSchemaService, offlineCompositionService,
    offlineSaveService, schemaService, contextService, routeService, tabsService,
    crudFilterContextService, validationService, crudContextHolderService, datamapSanitizationService, maximoDataService, menuModelService, loadingService, offlineAttachmentService) {

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

            createNewCompositionItem: function () {
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
            validateDetail: function (crudForm, displayables) {
                const crudContext = crudContextHolderService.getCrudContext();
                crudForm = crudForm || {};
                const detailSchema = this.currentDetailSchema();
                const datamap = crudContext.currentDetailItem.datamap;
                const toValidateDisplayables = displayables || detailSchema.displayables;
                return validationService.validate(detailSchema, toValidateDisplayables, datamap, crudForm.$error);
            },
            saveChanges: function (crudForm) {
                const crudContext = crudContextHolderService.getCrudContext();
                crudForm = crudForm || {};
                const validationErrors = this.validateDetail(crudForm);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return $q.reject(validationErrors);
                }

                const datamap = crudContext.currentDetailItem.datamap;
                if (crudContext.composition && crudContext.composition.currentDetailItem) {
                    const compositionItem = crudContext.composition.currentDetailItem;
                    return offlineSaveService.addAndSaveComposition(crudContext.currentApplicationName, crudContext.currentDetailItem, compositionItem, crudContext.composition.currentTab).then(() => {
                        crudContext.originalDetailItemDatamap = datamap;
                        crudContext.composition.originalDetailItemDatamap = crudContext.composition.currentDetailItem;
                        this.loadTab(crudContext.composition.currentTab);
                    });
                }

                return offlineSaveService.saveItem(crudContext.currentApplicationName, crudContext.currentDetailItem).then(() => {
                    crudContext.originalDetailItemDatamap = angular.copy(datamap);
                    contextService.insertIntoContext("crudcontext", crudContext);
                    if (crudContext.newItem) {
                        crudContext.newItem = false;
                        this.refreshGrid();
                    }
                });
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
                        return;
                    }
                    routeService.go("main.crudlist");
                    contextService.insertIntoContext("crudcontext", crudContext);
                });
            },

            loadMorePromise: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                const gridSearch = crudContextHolderService.getGridSearchData();
                const quickSearch = crudContextHolderService.getQuickSearch();
                const listSchema = crudContextHolderService.currentListSchema();
                const appName = crudContextHolderService.currentApplicationName();

                var baseQuery = "application = '{0}'".format(crudContext.currentApplicationName);
                if (quickSearch.value) {
                    baseQuery += ' and datamap like \'%:"{0}%\''.format(quickSearch.value);
                }
                if (!crudFilterContextService.showPending()) {
                    baseQuery += ' and pending = 0 ';
                }
                if (!crudFilterContextService.showDirty()) {
                    baseQuery += ' and isDirty = 0 ';
                }

                baseQuery += searchIndexService.buildSearchQuery(appName, listSchema, gridSearch);

                baseQuery += searchIndexService.buildSortQuery(appName, listSchema, gridSearch);

                return swdbDAO.findByQuery("DataEntry", baseQuery, { pagesize: 10, pageNumber: internalListContext.lastPageLoaded })
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


                crudContext.currentListSchema = offlineSchemaService.locateSchema(application, schemaId);
                crudContext.currentDetailSchema = offlineSchemaService.loadDetailSchema(crudContext.currentListSchema, crudContext.currentApplication);
                crudContext.currentNewDetailSchema = offlineSchemaService.locateSchemaByStereotype(crudContext.currentApplication, "detailnew");
                if (crudContext.currentNewDetailSchema == null && schemaService.isPropertyTrue(crudContext.currentDetailSchema, "mobile.actasnewschema")) {
                    //if this property is true, then the detail schema will also be used as the newschema
                    crudContext.currentNewDetailSchema = crudContext.currentDetailSchema;
                }
                this.refreshGrid();
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
                    routeService.go("main.crudlist");
                } else {
                    this.loadDetail(crudContext.previousItem);
                }

            },

            navigateNext: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                var outer = this;
                if (!crudContext.nextItem) {
                    return this.loadMorePromise().then(function (results) {
                        if (!results) {
                            //end has reached;
                            return;
                        }
                        crudContextHolderService.setPreviousAndNextItems(crudContext.currentDetailItem);
                        outer.loadDetail(crudContext.nextItem);
                    });
                }
                this.loadDetail(crudContext.nextItem);
                return $q.when();
            },
            
            isCreation: function () {
                const crudContext = this.getCrudContext();
                return !crudContext.originalDetailItemDatamap || crudContext.originalDetailItemDatamap["_newitem#$"];
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
                return routeService.go("main.cruddetail.maininput").then(function (result) {
                    $rootScope.$emit("sw_cruddetailrefreshed");
                    loadingService.hide();
                });

            },

            //#endregion
        }



    }]);

})(mobileServices, angular, constants);
