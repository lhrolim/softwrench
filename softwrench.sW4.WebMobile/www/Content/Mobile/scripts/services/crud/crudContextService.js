(function (mobileServices, angular, constants) {
    "use strict";
    constants = constants || {};

    mobileServices.factory('crudContextService', [
    "$q", "$log", "$rootScope", "swdbDAO",
    "metadataModelService", "offlineSchemaService", "offlineCompositionService",
    "offlineSaveService", "schemaService", "contextService", "routeService", "tabsService",
    "crudFilterContextService", "validationService", "crudContextHolderService", "datamapSanitizationService", "maximoDataService","menuModelService",
    function ($q, $log, $rootScope, swdbDAO,
    metadataModelService, offlineSchemaService, offlineCompositionService,
    offlineSaveService, schemaService, contextService, routeService, tabsService,
    crudFilterContextService, validationService, crudContextHolderService, datamapSanitizationService, maximoDataService,menuModelService) {

        // ReSharper disable once InconsistentNaming
        var internalListContext = {
            lastPageLoaded: 1
        }

        $rootScope.$on("sw4:security:logout", function () {
            metadataModelService.reset();
            crudContextHolderService.reset();
            menuModelService.reset();
        });
    

        return {

            //#region delegateMethods
            restoreState: function () {
                var savedState = crudContextHolderService.restoreState();
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

            getFilteredList: function () {
                return crudContextHolderService.getFilteredList();
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
                var detailSchema = this.currentDetailSchema();
                if (!detailSchema) {
                    return [];
                }
                var allDisplayables = tabsService.tabsDisplayables(detailSchema);
                return allDisplayables;
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

                return offlineCompositionService.loadComposition(crudContext.currentDetailItem, tab).then(function (compositionItems) {
                    crudContext.composition.currentTab = tab;
                    crudContext.composition.itemlist = compositionItems;
                    crudContext.composition.currentListSchema = tab.schema.schemas.list;
                    crudContext.composition.currentDetailSchema = tab.schema.schemas.detail;
                    crudContext.composition.currentDetailItem = null;
                    contextService.insertIntoContext("crudcontext", crudContext);
                    return routeService.go("main.cruddetail.compositionlist");
                });
            },

            loadCompositionDetail: function (item) {
                var crudContext = crudContextHolderService.getCrudContext();
                var fields = this.getCompositionDetailSchema().displayables;
                datamapSanitizationService.enforceNumericType(item, fields);
                //for compositions item will be the datamap itself
                crudContext.composition.currentDetailItem = item;
                crudContext.composition.originalDetailItemDatamap = angular.copy(item);
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.compositiondetail");
            },

            createNewCompositionItem: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                var compositionParentDatamap = crudContext.currentDetailItem.datamap;
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
                var crudContext = crudContextHolderService.getCrudContext();
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

            saveChanges: function (crudForm) {
                var crudContext = crudContextHolderService.getCrudContext();
                crudForm = crudForm || {};
                var detailSchema = this.currentDetailSchema();
                var datamap = crudContext.currentDetailItem.datamap;

                var validationErrors = validationService.validate(detailSchema, detailSchema.displayables, datamap, crudForm.$error);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return $q.when();
                }

                var that = this;
                if (crudContext.composition && crudContext.composition.currentDetailItem) {
                    var compositionItem = crudContext.composition.currentDetailItem;
                    return offlineSaveService.addAndSaveComposition(crudContext.currentApplicationName, crudContext.currentDetailItem, compositionItem, crudContext.composition.currentTab).then(function () {
                        crudContext.originalDetailItemDatamap = datamap;
                        crudContext.composition.originalDetailItemDatamap = crudContext.composition.currentDetailItem;
                        that.loadTab(crudContext.composition.currentTab);
                    });
                }

                return offlineSaveService.saveItem(crudContext.currentApplicationName, crudContext.currentDetailItem).then(function () {
                    crudContext.originalDetailItemDatamap = angular.copy(datamap);
                    contextService.insertIntoContext("crudcontext", crudContext);
                    if (crudContext.newItem) {
                        crudContext.newItem = false;
                        that.refreshGrid();
                    }
                });
            },

            //#endregion

            //#region GridFNS

            filterList: function (text) {
                var crudContext = crudContextHolderService.getCrudContext();
                internalListContext.searchQuery = text;
                if (text == null) {
                    return;
                }
                crudContext.filteredList = crudContext.filteredList || [];
                internalListContext.pageNumber = 1;
                internalListContext.lastPageLoaded = 1;
                this.loadMorePromise();
            },

            refreshGrid: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                internalListContext.searchQuery = null;
                crudContext.itemlist = [];
                internalListContext.lastPageLoaded = 1;
                internalListContext.pageNumber = 1;
                this.loadMorePromise().then(function () {
                    routeService.go("main.crudlist");
                    contextService.insertIntoContext("crudcontext", crudContext);
                });
            },

            loadMorePromise: function () {
                var crudContext = crudContextHolderService.getCrudContext();
                var baseQuery = "application = '{0}'".format(crudContext.currentApplicationName);
                var filteredMode = false;
                if (internalListContext.searchQuery != null) {
                    filteredMode = true;
                    baseQuery += ' and datamap like \'%:"{0}%\''.format(internalListContext.searchQuery);
                }
                if (!crudFilterContextService.showPending()) {
                    baseQuery += ' and pending = 0 ';
                }
                if (!crudFilterContextService.showDirty()) {
                    baseQuery += ' and isDirty = 0 ';
                }
                baseQuery += "order by rowstamp is null desc, rowstamp desc ";

                return swdbDAO.findByQuery("DataEntry", baseQuery, { pagesize: 10, pageNumber: internalListContext.lastPageLoaded })
                    .then(function (results) {
                        internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                        if (filteredMode) {
                            crudContext.filteredList = [];
                        }
                        var listToPush = filteredMode ? crudContext.filteredList : crudContext.itemlist;
                        for (var i = 0; i < results.length; i++) {
                            listToPush.push(results[i]);
                        }
                        return $q.when(results);
                    });
            },


            loadApplicationGrid: function (applicationName, applicationTitle, schemaId) {
                var crudContext = crudContextHolderService.getCrudContext();
                //cleaning up
                crudContext.currentDetailItem = null;
                crudContext.composition = {};

                var application = metadataModelService.getApplicationByName(applicationName);

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
                var crudContext = crudContextHolderService.getCrudContext();
                var newDetailSchema = crudContext.currentNewDetailSchema;
                return newDetailSchema != null;
            },

            //#endregion

            //#region detailFNs

            navigatePrevious: function () {
                var crudContext = crudContextHolderService.getCrudContext();
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

            createDetail: function () {
                var crudContext = crudContextHolderService.getCrudContext();
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
                var crudContext = crudContextHolderService.getCrudContext();
                /// <summary>
                ///  Loads a detail represented by the parameter item.
                /// </summary>
                /// <param name="item"></param>
                /// <returns type=""></returns>
                if (!crudContext.currentDetailSchema) {
                    crudContext.currentDetailSchema = offlineSchemaService.loadDetailSchema(crudContext.currentListSchema, crudContext.currentApplication, item);
                }

                var fields = this.mainDisplayables();
                datamapSanitizationService.enforceNumericType(item.datamap, fields);

                crudContext.currentDetailItem = item;
                crudContext.originalDetailItemDatamap = angular.copy(crudContext.currentDetailItem.datamap);
                crudContextHolderService.setPreviousAndNextItems(item);
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.maininput").then(function (result) {
                    $rootScope.$emit("sw_cruddetailrefreshed");
                });
            },

            //#endregion






        }



    }]);

})(mobileServices, angular, constants);